using GameZone.Domain.Entities;
using GameZone.Domain.Enums;
using GameZone.Domain.Interfaces;
using GameZone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameZone.Infrastructure.Repositories;

public class SessionRepository : Repository<Session>, ISessionRepository
{
    public SessionRepository(GameZoneDbContext context) : base(context)
    {
    }

    public async Task<Session?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        => await Set
            .Include(s => s.Client)
            .Include(s => s.GamingItem)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Session>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Include(s => s.Client)
            .Include(s => s.GamingItem)
            .Include(s => s.Payments)
            .Where(s => s.SessionStatus == SessionStatus.Active)
            .OrderByDescending(s => s.EntryTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Session>> GetTodaysSessionsAsync(DateTime today, CancellationToken cancellationToken = default)
    {
        var start = today.Date;
        var end = start.AddDays(1);
        return await Set.AsNoTracking()
            .Include(s => s.Client)
            .Include(s => s.GamingItem)
            .Include(s => s.Payments)
            .Where(s => s.EntryTime >= start && s.EntryTime < end)
            .OrderByDescending(s => s.EntryTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Session>> GetByClientAsync(int clientId, CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Include(s => s.GamingItem)
            .Include(s => s.Payments)
            .Where(s => s.ClientId == clientId)
            .OrderByDescending(s => s.EntryTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Session>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Include(s => s.Client)
            .Include(s => s.GamingItem)
            .Include(s => s.Payments)
            .Where(s => s.EntryTime >= from && s.EntryTime < to)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Session>> GetByStatusAsync(SessionStatus status, CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Include(s => s.Client)
            .Include(s => s.GamingItem)
            .Include(s => s.Payments)
            .Where(s => s.SessionStatus == status)
            .ToListAsync(cancellationToken);

    public async Task<int> CountDistinctClientsOnDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return await Set
            .Where(s => s.EntryTime >= start && s.EntryTime < end)
            .Select(s => s.ClientId)
            .Distinct()
            .CountAsync(cancellationToken);
    }
}
