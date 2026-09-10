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
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapFallbackToFile("index.html");

app.Run();

static string ResolveConnectionString(IConfiguration configuration)
{
    var postgres = configuration["DATABASE_URL"]
        ?? configuration.GetConnectionString("Postgres")
        ?? Environment.GetEnvironmentVariable("DATABASE_URL");
    postgres = NormalizePostgres(postgres);
    if (!string.IsNullOrWhiteSpace(postgres) && CanOpenPostgres(postgres))
    {
        Console.WriteLine("Using online PostgreSQL.");
        return postgres;
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
    Console.WriteLine($"PostgreSQL is not reachable. Using local SQLite at {sqlitePath}");
    return $"Data Source={sqlitePath}";
}

static string? NormalizePostgres(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
        return value;

    if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        && !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        return value;

    var uri = new Uri(value);
    var userInfo = uri.UserInfo.Split(':', 2);
    var user = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var database = uri.AbsolutePath.Trim('/');
    return $"Host={uri.Host};Port={(uri.Port > 0 ? uri.Port : 5432)};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}

static bool CanOpenPostgres(string connectionString)
{
    try
    {
        using var connection = new Npgsql.NpgsqlConnection(connectionString);
        connection.Open();
        return true;
    }
    catch
    {
        return false;
    }
}
