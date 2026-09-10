using GameZone.Domain.Entities;
using GameZone.Domain.Enums;

namespace GameZone.Domain.Interfaces;

public interface ISessionRepository : IRepository<Session>
{
    Task<Session?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Session>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Session>> GetTodaysSessionsAsync(DateTime today, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Session>> GetByClientAsync(int clientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Session>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Session>> GetByStatusAsync(SessionStatus status, CancellationToken cancellationToken = default);
    Task<int> CountDistinctClientsOnDateAsync(DateTime date, CancellationToken cancellationToken = default);
}
