using GameZone.Domain.Entities;
using GameZone.Domain.Enums;
using GameZone.Domain.Interfaces;
using GameZone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameZone.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(GameZoneDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => await Set.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower(), cancellationToken);

    public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null, CancellationToken cancellationToken = default)
        => await Set.AnyAsync(u => u.Username.ToLower() == username.ToLower() && (!excludeId.HasValue || u.Id != excludeId), cancellationToken);

    public async Task<IReadOnlyList<User>> GetEmployeesAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Where(u => u.Role == UserRole.Employee)
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
}
