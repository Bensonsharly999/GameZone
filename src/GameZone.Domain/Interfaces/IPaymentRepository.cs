using GameZone.Domain.Entities;
using GameZone.Domain.Enums;

namespace GameZone.Domain.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<Payment?> GetBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default);
    Task<decimal> SumPaidOnDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<decimal> SumPaidBetweenAsync(DateTime fromInclusive, DateTime toExclusive, CancellationToken cancellationToken = default);
    Task<int> CountPendingAsync(CancellationToken cancellationToken = default);
}
