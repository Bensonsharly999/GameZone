using GameZone.Application.DTOs.Dashboard;
using GameZone.Application.Interfaces;
using GameZone.Domain.Enums;
using GameZone.Domain.Interfaces;

namespace GameZone.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        return new DashboardStatsDto
        {
            TodaysClientsCount = await _unitOfWork.Sessions.CountDistinctClientsOnDateAsync(today, cancellationToken),
            ActiveSessions = await _unitOfWork.Sessions.CountAsync(s => s.SessionStatus == SessionStatus.Active, cancellationToken),
            TodaysRevenue = await _unitOfWork.Payments.SumPaidOnDateAsync(today, cancellationToken),
            MonthlyRevenue = await _unitOfWork.Payments.SumPaidBetweenAsync(monthStart, monthStart.AddMonths(1), cancellationToken),
            MonthLabel = today.ToString("MMMM yyyy"),
            PendingPayments = await _unitOfWork.Payments.CountPendingAsync(cancellationToken)
        };
    }
}
