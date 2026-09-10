using Microsoft.EntityFrameworkCore;

namespace GameZone.Infrastructure.Data;

public static class SchemaPatcher
{
    public static async Task ApplyAsync(GameZoneDbContext context, CancellationToken cancellationToken = default)
    {
        var provider = context.Database.ProviderName ?? string.Empty;
        if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            await AddSqliteColumnIfMissingAsync(context, "GamingItems", "Rate30MinOnePlayer", "TEXT NOT NULL DEFAULT '80'", cancellationToken);
            await AddSqliteColumnIfMissingAsync(context, "GamingItems", "Rate30MinTwoPlayers", "TEXT NOT NULL DEFAULT '120'", cancellationToken);
            await AddSqliteColumnIfMissingAsync(context, "GamingItems", "RateOneHourOnePlayer", "TEXT NOT NULL DEFAULT '150'", cancellationToken);
            await AddSqliteColumnIfMissingAsync(context, "GamingItems", "RateOneHourTwoPlayers", "TEXT NOT NULL DEFAULT '200'", cancellationToken);
            await AddSqliteColumnIfMissingAsync(context, "Sessions", "PlayerCount", "INTEGER NOT NULL DEFAULT 1", cancellationToken);
            return;
        }

        await context.Database.ExecuteSqlRawAsync(
            """ALTER TABLE "GamingItems" ADD COLUMN IF NOT EXISTS "Rate30MinOnePlayer" numeric(10,2) NOT NULL DEFAULT 80;""", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            """ALTER TABLE "GamingItems" ADD COLUMN IF NOT EXISTS "Rate30MinTwoPlayers" numeric(10,2) NOT NULL DEFAULT 120;""", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            """ALTER TABLE "GamingItems" ADD COLUMN IF NOT EXISTS "RateOneHourOnePlayer" numeric(10,2) NOT NULL DEFAULT 150;""", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            """ALTER TABLE "GamingItems" ADD COLUMN IF NOT EXISTS "RateOneHourTwoPlayers" numeric(10,2) NOT NULL DEFAULT 200;""", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            """ALTER TABLE "Sessions" ADD COLUMN IF NOT EXISTS "PlayerCount" integer NOT NULL DEFAULT 1;""", cancellationToken);
    }

    private static async Task AddSqliteColumnIfMissingAsync(
        GameZoneDbContext context, string table, string column, string sqlType, CancellationToken cancellationToken)
    {
        var exists = false;
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = $"PRAGMA table_info('{table}')";
        if (command.Connection!.State != System.Data.ConnectionState.Open)
            await command.Connection.OpenAsync(cancellationToken);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (string.Equals(reader["name"]?.ToString(), column, StringComparison.OrdinalIgnoreCase))
            {
                exists = true;
                break;
            }
        }

        if (!exists)
            await context.Database.ExecuteSqlRawAsync($"ALTER TABLE {table} ADD COLUMN {column} {sqlType}", cancellationToken);
    }
}
