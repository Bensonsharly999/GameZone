using GameZone.Application.Common;
using GameZone.Application.DTOs.Reports;
using GameZone.Application.Interfaces;
using GameZone.Domain.Enums;
using GameZone.Domain.Interfaces;

namespace GameZone.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DailyRevenueDto>> GetDailyRevenueAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var end = to.Date.AddDays(1);
        var sessions = await _unitOfWork.Sessions.GetByDateRangeAsync(from.Date, end, cancellationToken);
        var payments = await _unitOfWork.Payments.GetAllWithDetailsAsync(cancellationToken);

        var paidByDay = payments
            .Where(p => p.PaymentStatus == PaymentStatus.Paid && p.PaymentDate >= from.Date && p.PaymentDate < end)
            .GroupBy(p => p.PaymentDate.Date)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

        var clientsByDay = sessions
            .GroupBy(s => s.EntryTime.Date)
            .ToDictionary(g => g.Key, g => g.Select(s => s.ClientId).Distinct().Count());

        var days = new List<DailyRevenueDto>();
        for (var day = from.Date; day < end; day = day.AddDays(1))
        {
            days.Add(new DailyRevenueDto
            {
                Date = day,
                TotalRevenue = paidByDay.TryGetValue(day, out var rev) ? rev : 0,
                TotalClients = clientsByDay.TryGetValue(day, out var count) ? count : 0
            });
        }

        return days.OrderByDescending(d => d.Date).ToList();
    }

    public async Task<IReadOnlyList<RevenueByItemDto>> GetRevenueByItemAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var start = from?.Date ?? CafeClock.Today.AddMonths(-1);
        var end = (to?.Date ?? CafeClock.Today).AddDays(1);
        var sessions = await _unitOfWork.Sessions.GetByDateRangeAsync(start, end, cancellationToken);

        return sessions
            .Where(s => s.SessionStatus == SessionStatus.Completed)
            .GroupBy(s => s.GamingItem?.Name ?? "Unknown")
            .Select(g => new RevenueByItemDto
            {
                ItemName = g.Key,
                TotalSessions = g.Count(),
                Revenue = g.Sum(s => s.Amount ?? 0)
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();
    }

    public async Task<IReadOnlyList<TopClientDto>> GetTopClientsAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var clients = await _unitOfWork.Clients.GetAllAsync(cancellationToken);
        var result = new List<TopClientDto>();

        foreach (var client in clients)
        {
            var withSessions = await _unitOfWork.Clients.GetWithSessionsAsync(client.Id, cancellationToken);
            if (withSessions is null)
                continue;

            result.Add(new TopClientDto
            {
                ClientName = withSessions.Name,
                PhoneNumber = withSessions.PhoneNumber,
                TotalVisits = withSessions.Sessions.Count,
                TotalAmountSpent = withSessions.Sessions
                    .Where(s => s.SessionStatus == SessionStatus.Completed)
                    .Sum(s => s.Amount ?? 0)
            });
        }

        return result
            .Where(c => c.TotalVisits > 0)
            .OrderByDescending(c => c.TotalAmountSpent)
            .ThenByDescending(c => c.TotalVisits)
            .Take(take)
            .ToList();
    }

    public async Task<IReadOnlyList<PendingPaymentReportDto>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.Payments.GetByStatusAsync(PaymentStatus.Pending, cancellationToken);
        return payments
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new PendingPaymentReportDto
            {
                ClientName = p.Session?.Client?.Name ?? "-",
                PhoneNumber = p.Session?.Client?.PhoneNumber ?? "-",
                Amount = p.Amount,
                Date = p.PaymentDate,
                GamingItem = p.Session?.GamingItem?.Name ?? "-"
            })
            .ToList();
    }
}
