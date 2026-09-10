namespace GameZone.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TodaysClientsCount { get; set; }
    public int ActiveSessions { get; set; }
    public decimal TodaysRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public int PendingPayments { get; set; }
}
