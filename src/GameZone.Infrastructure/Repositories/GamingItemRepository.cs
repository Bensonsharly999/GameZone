using GameZone.Domain.Entities;
using GameZone.Domain.Interfaces;
using GameZone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameZone.Infrastructure.Repositories;

public class GamingItemRepository : Repository<GamingItem>, IGamingItemRepository
{
    public GamingItemRepository(GameZoneDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<GamingItem>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Where(i => i.IsActive)
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken);

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
        => await Set.AnyAsync(i => i.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || i.Id != excludeId), cancellationToken);

    public async Task<bool> HasSessionsAsync(int id, CancellationToken cancellationToken = default)
        => await Context.Sessions.AnyAsync(s => s.GamingItemId == id, cancellationToken);
}
