using GameZone.Domain.Entities;
using GameZone.Domain.Interfaces;
using GameZone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameZone.Infrastructure.Repositories;

public class ClientRepository : Repository<Client>, IClientRepository
{
    public ClientRepository(GameZoneDbContext context) : base(context)
    {
    }

    public async Task<Client?> GetByPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default)
        => await Set.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber, cancellationToken);

    public async Task<bool> PhoneExistsAsync(string phoneNumber, int? excludeId = null, CancellationToken cancellationToken = default)
        => await Set.AnyAsync(c => c.PhoneNumber == phoneNumber && (!excludeId.HasValue || c.Id != excludeId), cancellationToken);

    public async Task<IReadOnlyList<Client>> SearchAsync(string term, CancellationToken cancellationToken = default)
        => await SearchWithSessionsAsync(term, cancellationToken);

    public async Task<IReadOnlyList<Client>> SearchWithSessionsAsync(string? term, CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking()
            .Include(c => c.Sessions)
                .ThenInclude(s => s.Payments)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(term))
        {
            var lowered = term.Trim().ToLower();
            var phone = term.Trim();
            query = query.Where(c => c.Name.ToLower().Contains(lowered) || c.PhoneNumber.Contains(phone));
        }

        return await query.OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<Client?> GetWithSessionsAsync(int id, CancellationToken cancellationToken = default)
        => await Set
            .Include(c => c.Sessions)
                .ThenInclude(s => s.GamingItem)
            .Include(c => c.Sessions)
                .ThenInclude(s => s.Payments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}
