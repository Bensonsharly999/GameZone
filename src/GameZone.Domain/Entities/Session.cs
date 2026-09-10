using GameZone.Domain.Enums;

namespace GameZone.Domain.Entities;

public class Session
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int GamingItemId { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public int? DurationMinutes { get; set; }
    public decimal? Amount { get; set; }
    public SessionStatus SessionStatus { get; set; } = SessionStatus.Active;
    public string? Notes { get; set; }
    public int PlayerCount { get; set; } = 1;

    public Client Client { get; set; } = null!;
    public GamingItem GamingItem { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
