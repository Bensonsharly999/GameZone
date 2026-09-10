namespace GameZone.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IClientRepository Clients { get; }
    IGamingItemRepository GamingItems { get; }
    ISessionRepository Sessions { get; }
    IPaymentRepository Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
