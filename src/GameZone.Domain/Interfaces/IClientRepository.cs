using GameZone.Domain.Entities;

namespace GameZone.Domain.Interfaces;

public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetByPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<bool> PhoneExistsAsync(string phoneNumber, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Client>> SearchAsync(string term, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Client>> SearchWithSessionsAsync(string? term, CancellationToken cancellationToken = default);
    Task<Client?> GetWithSessionsAsync(int id, CancellationToken cancellationToken = default);
}
