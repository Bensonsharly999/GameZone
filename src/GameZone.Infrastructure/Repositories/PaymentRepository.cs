using GameZone.Domain.Entities;
using GameZone.Domain.Enums;
using GameZone.Domain.Interfaces;
using GameZone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameZone.Infrastructure.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(GameZoneDbContext context) : base(context)
    {
    }

    public async Task<Payment?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        => await Set
            .Include(p => p.Session).ThenInclude(s => s.Client)
            .Include(p => p.Session).ThenInclude(s => s.GamingItem)
            .Include(p => p.ReceivedByUser)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Payment>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Include(p => p.Session).ThenInclude(s => s.Client)
            .Include(p => p.Session).ThenInclude(s => s.GamingItem)
            .Include(p => p.ReceivedByUser)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Include(p => p.Session).ThenInclude(s => s.Client)
            .Include(p => p.Session).ThenInclude(s => s.GamingItem)
            .Include(p => p.ReceivedByUser)
            .Where(p => p.PaymentStatus == status)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Payment>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return await Set.AsNoTracking()
            .Include(p => p.Session).ThenInclude(s => s.Client)
            .Include(p => p.ReceivedByUser)
            .Where(p => p.PaymentDate >= start && p.PaymentDate < end)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment?> GetBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default)
        => await Set.FirstOrDefaultAsync(p => p.SessionId == sessionId, cancellationToken);

    public async Task<decimal> SumPaidOnDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        var amounts = await Set.AsNoTracking()
            .Where(p => p.PaymentStatus == PaymentStatus.Paid && p.PaymentDate >= start && p.PaymentDate < end)
            .Select(p => p.Amount)
            .ToListAsync(cancellationToken);
        return amounts.Sum();
    }

    public async Task<decimal> SumPaidBetweenAsync(DateTime fromInclusive, DateTime toExclusive, CancellationToken cancellationToken = default)
    {
        var amounts = await Set.AsNoTracking()
            .Where(p => p.PaymentStatus == PaymentStatus.Paid && p.PaymentDate >= fromInclusive && p.PaymentDate < toExclusive)
            .Select(p => p.Amount)
            .ToListAsync(cancellationToken);
        return amounts.Sum();
    }

    public async Task<int> CountPendingAsync(CancellationToken cancellationToken = default)
        => await Set.CountAsync(p => p.PaymentStatus == PaymentStatus.Pending, cancellationToken);
}
