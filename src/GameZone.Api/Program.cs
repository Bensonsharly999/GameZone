using System.Text;
using GameZone.Api.Auth;
using GameZone.Application;
using GameZone.Application.Common;
using GameZone.Application.Interfaces;
using GameZone.Infrastructure;
using GameZone.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
    var db = scope.ServiceProvider.GetRequiredService<GameZoneDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbInitializer.SeedAsync(db, hasher);
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
        var opened = TryOpenPostgres(postgres, retries: onRender ? 5 : 2);
        if (opened is not null)
        {
            Console.WriteLine("Using PostgreSQL (data is kept across restarts).");
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
        return EnsureSslPrefer(value);

    try
    {
        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var database = uri.AbsolutePath.Trim('/');
        return EnsureSslPrefer(
            $"Host={uri.Host};Port={(uri.Port > 0 ? uri.Port : 5432)};Database={database};Username={user};Password={password}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Invalid DATABASE_URL: {ex.Message}");
        return null;
    }
}

static string EnsureSslPrefer(string connectionString)
{
    if (connectionString.Contains("SSL Mode", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("Ssl Mode", StringComparison.OrdinalIgnoreCase))
        return connectionString;

    return connectionString.TrimEnd(';') + ";SSL Mode=Prefer;Trust Server Certificate=true";
}

static string? TryOpenPostgres(string connectionString, int retries)
{
    var variants = new[]
    {
        connectionString,
        connectionString.Replace("SSL Mode=Require", "SSL Mode=Prefer", StringComparison.OrdinalIgnoreCase),
        connectionString.Replace("SSL Mode=Prefer", "SSL Mode=Disable", StringComparison.OrdinalIgnoreCase)
            .Replace("SSL Mode=Require", "SSL Mode=Disable", StringComparison.OrdinalIgnoreCase)
    }.Distinct(StringComparer.OrdinalIgnoreCase);

    for (var attempt = 1; attempt <= retries; attempt++)
    {
        foreach (var candidate in variants)
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(candidate);
                connection.Open();
                return candidate;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PostgreSQL attempt {attempt}: {ex.Message}");
            }
        }

        Thread.Sleep(2000);
    }

    return null;
}
