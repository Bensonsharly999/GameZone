using System.Text;
using GameZone.Api.Auth;
using GameZone.Application;
using GameZone.Application.Common;
using GameZone.Application.Interfaces;
using GameZone.Infrastructure;
using GameZone.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

LoadDotEnv();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
BindRenderPort(builder);

var connectionString = ResolveConnectionString(builder.Configuration);

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();
builder.Services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new IstDateTimeJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new IstNullableDateTimeJsonConverter());
    });
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<GameZoneDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await DbInitializer.SeedAsync(db, hasher);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Database startup failed: " + ex);
        throw;
    }
}

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CurrentUserMiddleware>();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    storage = connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) ? "sqlite" : "postgres"
}));
app.MapFallbackToFile("index.html");

app.Run();

static void LoadDotEnv()
{
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    for (var i = 0; i < 8 && dir is not null; i++, dir = dir.Parent)
        ApplyEnvFile(Path.Combine(dir.FullName, ".env"));
    ApplyEnvFile(Path.Combine(AppContext.BaseDirectory, ".env"));
}

static void ApplyEnvFile(string path)
{
    if (!File.Exists(path))
        return;

    foreach (var raw in File.ReadAllLines(path))
    {
        var line = raw.Trim();
        if (line.Length == 0 || line.StartsWith('#') || !line.Contains('='))
            continue;
        var split = line.Split('=', 2);
        var key = split[0].Trim();
        var value = split[1].Trim().Trim('"').Trim('\'');
        if (!string.IsNullOrEmpty(key) && string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            Environment.SetEnvironmentVariable(key, value);
    }
}

static void BindRenderPort(WebApplicationBuilder builder)
{
    var port = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrWhiteSpace(port))
        builder.WebHost.UseUrls($"http://0.0.0.0:{port.Trim()}");
}

static string ResolveConnectionString(IConfiguration configuration)
{
    var onRender = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RENDER"));
    var postgres = configuration["DATABASE_URL"]
        ?? Environment.GetEnvironmentVariable("DATABASE_URL");
    if (string.IsNullOrWhiteSpace(postgres) && !onRender)
        postgres = configuration.GetConnectionString("Postgres");

    postgres = NormalizePostgres(postgres);
    if (!string.IsNullOrWhiteSpace(postgres))
    {
        var opened = TryOpenPostgres(postgres, retries: onRender ? 8 : 3);
        if (opened is not null)
        {
            Console.WriteLine($"Using PostgreSQL host={HostOf(opened)} (data is kept across restarts).");
            return opened;
        }

        Console.WriteLine("DATABASE_URL did not open. Falling back to SQLite so the site can start.");
    }
    else if (onRender)
    {
        Console.WriteLine("WARNING: DATABASE_URL is missing. Render will wipe SQLite when the service sleeps. Add a Postgres DATABASE_URL to keep cafe data.");
    }

    var dataDir = configuration["GAMEZONE_DATA_DIR"]
        ?? Environment.GetEnvironmentVariable("GAMEZONE_DATA_DIR");
    if (string.IsNullOrWhiteSpace(dataDir))
    {
        dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameZoneMobile");
    }

    Directory.CreateDirectory(dataDir);
    var sqlitePath = Path.Combine(dataDir, "GameZone.db");
    Console.WriteLine($"Using local SQLite at {sqlitePath}");
    return $"Data Source={sqlitePath}";
}

static string? NormalizePostgres(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
        return value;

    if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        && !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        return HardenPostgres(value);

    try
    {
        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var database = uri.AbsolutePath.Trim('/');
        return HardenPostgres(
            $"Host={uri.Host};Port={(uri.Port > 0 ? uri.Port : 5432)};Database={database};Username={user};Password={password}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Invalid DATABASE_URL: {ex.Message}");
        return null;
    }
}

static string HardenPostgres(string connectionString)
{
    var parts = connectionString.Trim().TrimEnd(';');
    if (!ContainsKey(parts, "SSL Mode") && !ContainsKey(parts, "Ssl Mode"))
        parts += ";SSL Mode=Require";
    if (!ContainsKey(parts, "Trust Server Certificate"))
        parts += ";Trust Server Certificate=true";
    if (!ContainsKey(parts, "Channel Binding"))
        parts += ";Channel Binding=Disable";
    if (!ContainsKey(parts, "Timeout"))
        parts += ";Timeout=30";
    if (!ContainsKey(parts, "Command Timeout"))
        parts += ";Command Timeout=60";
    if (!ContainsKey(parts, "Pooling"))
        parts += ";Pooling=true";
    return parts;
}

static bool ContainsKey(string connectionString, string key)
    => connectionString.Contains(key + "=", StringComparison.OrdinalIgnoreCase);

static string HostOf(string connectionString)
{
    foreach (var part in connectionString.Split(';'))
    {
        var kv = part.Split('=', 2);
        if (kv.Length == 2 && kv[0].Trim().Equals("Host", StringComparison.OrdinalIgnoreCase))
            return kv[1].Trim();
    }
    return "(unknown)";
}

static string DirectNeonHost(string connectionString)
{
    foreach (var part in connectionString.Split(';'))
    {
        var kv = part.Split('=', 2);
        if (kv.Length == 2 && kv[0].Trim().Equals("Host", StringComparison.OrdinalIgnoreCase)
            && kv[1].Contains("-pooler.", StringComparison.OrdinalIgnoreCase))
        {
            var direct = kv[1].Replace("-pooler.", ".", StringComparison.OrdinalIgnoreCase);
            return connectionString.Replace(kv[1], direct, StringComparison.OrdinalIgnoreCase);
        }
    }
    return connectionString;
}

static string? TryOpenPostgres(string connectionString, int retries)
{
    var hosts = new List<string> { DirectNeonHost(connectionString), connectionString };
    var variants = new List<string>();
    foreach (var host in hosts.Distinct(StringComparer.OrdinalIgnoreCase))
    {
        variants.Add(host);
        variants.Add(host.Replace("Pooling=true", "Pooling=false", StringComparison.OrdinalIgnoreCase));
    }

    for (var attempt = 1; attempt <= retries; attempt++)
    {
        foreach (var candidate in variants.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(candidate);
                connection.Open();
                return candidate;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PostgreSQL attempt {attempt} ({HostOf(candidate)}): {ex.Message}");
            }
        }

        Thread.Sleep(2000);
    }

    return null;
}
