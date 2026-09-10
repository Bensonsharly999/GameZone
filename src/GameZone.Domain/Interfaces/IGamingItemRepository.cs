using GameZone.Domain.Entities;

namespace GameZone.Domain.Interfaces;

public interface IGamingItemRepository : IRepository<GamingItem>
{
    Task<IReadOnlyList<GamingItem>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> HasSessionsAsync(int id, CancellationToken cancellationToken = default);
}
