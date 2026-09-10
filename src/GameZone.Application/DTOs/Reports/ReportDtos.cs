namespace GameZone.Application.DTOs.Reports;

public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalClients { get; set; }
}

public class RevenueByItemDto
{
    public string ItemName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public decimal Revenue { get; set; }
}

public class TopClientDto
{
    public string ClientName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int TotalVisits { get; set; }
    public decimal TotalAmountSpent { get; set; }
}

public class PendingPaymentReportDto
{
    public string ClientName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string GamingItem { get; set; } = string.Empty;
}
