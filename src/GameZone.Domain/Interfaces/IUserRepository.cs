using GameZone.Domain.Entities;

namespace GameZone.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetEmployeesAsync(CancellationToken cancellationToken = default);
}
