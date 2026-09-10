using GameZone.Application.DTOs.Reports;

namespace GameZone.Application.Interfaces;

public interface IReportService
{
    Task<IReadOnlyList<DailyRevenueDto>> GetDailyRevenueAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RevenueByItemDto>> GetRevenueByItemAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopClientDto>> GetTopClientsAsync(int take = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PendingPaymentReportDto>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);
}
