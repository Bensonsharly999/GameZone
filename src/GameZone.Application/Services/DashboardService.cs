using GameZone.Application.Common;
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
        var (todayStart, todayEnd) = CafeClock.UtcDayRangeIst();
        var (monthStart, monthEnd) = CafeClock.UtcMonthRangeIst();
        var todaysSessions = await _unitOfWork.Sessions.GetByDateRangeAsync(todayStart, todayEnd, cancellationToken);
        return new DashboardStatsDto
        {
            TodaysClientsCount = todaysSessions.Select(s => s.ClientId).Distinct().Count(),
            ActiveSessions = await _unitOfWork.Sessions.CountAsync(s => s.SessionStatus == SessionStatus.Active, cancellationToken),
            TodaysRevenue = await _unitOfWork.Payments.SumPaidBetweenAsync(todayStart, todayEnd, cancellationToken),
            MonthlyRevenue = await _unitOfWork.Payments.SumPaidBetweenAsync(monthStart, monthEnd, cancellationToken),
            MonthLabel = CafeClock.Today.ToString("MMMM yyyy"),
            PendingPayments = await _unitOfWork.Payments.CountPendingAsync(cancellationToken)
        };
    }
}
