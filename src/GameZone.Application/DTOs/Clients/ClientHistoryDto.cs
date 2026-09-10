namespace GameZone.Application.DTOs.Clients;

public class ClientHistoryDto
{
    public ClientDto Client { get; set; } = new();
    public IReadOnlyList<ClientHistoryItemDto> Visits { get; set; } = Array.Empty<ClientHistoryItemDto>();
}

public class ClientHistoryItemDto
{
    public DateTime VisitDate { get; set; }
    public string GamingItem { get; set; } = string.Empty;
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public string Duration { get; set; } = "-";
    public decimal? Amount { get; set; }
    public string PaymentMethod { get; set; } = "-";
    public string PaymentStatus { get; set; } = "-";
}
