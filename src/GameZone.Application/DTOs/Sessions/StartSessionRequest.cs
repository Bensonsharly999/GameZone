namespace GameZone.Application.DTOs.Sessions;

public class StartSessionRequest
{
    public int ClientId { get; set; }
    public int GamingItemId { get; set; }
    public string? Notes { get; set; }
    public int PlayerCount { get; set; } = 1;
}
