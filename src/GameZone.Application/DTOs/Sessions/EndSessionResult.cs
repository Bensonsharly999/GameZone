namespace GameZone.Application.DTOs.Sessions;

public class EndSessionResult
{
    public SessionDto Session { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public decimal RatePerHour { get; set; }
    public decimal Amount { get; set; }
    public decimal HalfHourAmount { get; set; }
}
