using GameZone.Application.Interfaces;
using GameZone.Domain.Entities;
using GameZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GameZone.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(GameZoneDbContext context, IPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        await context.Database.EnsureCreatedAsync(cancellationToken);
        await SchemaPatcher.ApplyAsync(context, cancellationToken);

        if (!await context.Users.AnyAsync(cancellationToken))
        {
            context.Users.Add(new User
            {
                Name = "System Administrator",
                Username = "admin",
                PasswordHash = passwordHasher.Hash("admin123"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            });
        }

        if (!await context.GamingItems.AnyAsync(cancellationToken))
        {
            context.GamingItems.AddRange(
                CreateItem("PS5"),
                CreateItem("VR"),
                CreateItem("Steering Wheel Racing"),
                CreateItem("Gaming PC"));
        }

        foreach (var item in await context.GamingItems.ToListAsync(cancellationToken))
            ApplyDefaultRates(item);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static GamingItem CreateItem(string name) => new()
    {
        Name = name,
        IsActive = true,
        Rate30MinOnePlayer = 80m,
        Rate30MinTwoPlayers = 120m,
        RateOneHourOnePlayer = 150m,
        RateOneHourTwoPlayers = 200m,
        RatePerHour = 150m
    };

    private static void ApplyDefaultRates(GamingItem item)
    {
        if (item.Rate30MinOnePlayer <= 0) item.Rate30MinOnePlayer = 80m;
        if (item.Rate30MinTwoPlayers <= 0) item.Rate30MinTwoPlayers = 120m;
        if (item.RateOneHourOnePlayer <= 0) item.RateOneHourOnePlayer = 150m;
        if (item.RateOneHourTwoPlayers <= 0) item.RateOneHourTwoPlayers = 200m;
        if (item.RatePerHour <= 0) item.RatePerHour = item.RateOneHourOnePlayer;
    }
}
