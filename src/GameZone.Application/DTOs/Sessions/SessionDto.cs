using GameZone.Domain.Enums;

namespace GameZone.Application.DTOs.Sessions;

public class SessionDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int GamingItemId { get; set; }
    public string GamingItemName { get; set; } = string.Empty;
    public decimal RatePerHour { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public int? DurationMinutes { get; set; }
    public decimal? Amount { get; set; }
    public SessionStatus SessionStatus { get; set; }
    public string? Notes { get; set; }
    public int PlayerCount { get; set; } = 1;
    public string PlayersText => PlayerCount <= 1 ? "1 player" : $"{PlayerCount} players";
    public string PaymentStatus { get; set; } = "None";
    public string PaymentMethod { get; set; } = "-";
    public int VisitNumber { get; set; }
    public bool FreeEligible { get; set; }

    public string DurationDisplay => DurationMinutes is null
        ? "-"
        : FormatDuration(DurationMinutes.Value);

    public string StatusText => SessionStatus.ToString();

    public static string FormatDuration(int minutes)
    {
        var hours = minutes / 60;
        var mins = minutes % 60;
        return hours > 0 ? $"{hours}h {mins}m" : $"{mins}m";
    }
}
