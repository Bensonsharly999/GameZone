using GameZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameZone.Infrastructure.Data;

public class GameZoneDbContext : DbContext
{
    public GameZoneDbContext(DbContextOptions<GameZoneDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<GamingItem> GamingItems => Set<GamingItem>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
            entity.Property(x => x.Username).IsRequired().HasMaxLength(80);
            entity.Property(x => x.PasswordHash).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
            entity.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(x => x.Address).HasMaxLength(250);
            entity.HasIndex(x => x.PhoneNumber).IsUnique();
        });

        modelBuilder.Entity<GamingItem>(entity =>
        {
            entity.ToTable("GamingItems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
            entity.Property(x => x.RatePerHour).HasPrecision(10, 2);
            entity.Property(x => x.Rate30MinOnePlayer).HasPrecision(10, 2);
            entity.Property(x => x.Rate30MinTwoPlayers).HasPrecision(10, 2);
            entity.Property(x => x.RateOneHourOnePlayer).HasPrecision(10, 2);
            entity.Property(x => x.RateOneHourTwoPlayers).HasPrecision(10, 2);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("Sessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Amount).HasPrecision(10, 2);
            entity.Property(x => x.SessionStatus).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Notes).HasMaxLength(500);

            entity.HasOne(x => x.Client)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.GamingItem)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.GamingItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Amount).HasPrecision(10, 2);
            entity.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.PaymentStatus).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.TransactionReference).HasMaxLength(120);

            entity.HasOne(x => x.Session)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReceivedByUser)
                .WithMany(x => x.ReceivedPayments)
                .HasForeignKey(x => x.ReceivedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
