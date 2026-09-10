using GameZone.Domain.Interfaces;
using GameZone.Infrastructure.Data;

namespace GameZone.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly GameZoneDbContext _context;

    public UnitOfWork(GameZoneDbContext context)
    {
        _context = context;
        Users = new UserRepository(_context);
        Clients = new ClientRepository(_context);
        GamingItems = new GamingItemRepository(_context);
        Sessions = new SessionRepository(_context);
        Payments = new PaymentRepository(_context);
    }

    public IUserRepository Users { get; }
    public IClientRepository Clients { get; }
    public IGamingItemRepository GamingItems { get; }
    public ISessionRepository Sessions { get; }
    public IPaymentRepository Payments { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
