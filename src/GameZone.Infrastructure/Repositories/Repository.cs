using System.Linq.Expressions;
using GameZone.Domain.Interfaces;
using GameZone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameZone.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly GameZoneDbContext Context;
    protected readonly DbSet<T> Set;

    public Repository(GameZoneDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await Set.FindAsync(new object[] { id }, cancellationToken);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await Set.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await Set.FirstOrDefaultAsync(predicate, cancellationToken);

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await Set.AnyAsync(predicate, cancellationToken);

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        => predicate is null
            ? await Set.CountAsync(cancellationToken)
            : await Set.CountAsync(predicate, cancellationToken);

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await Set.AddAsync(entity, cancellationToken);

    public virtual void Update(T entity) => Set.Update(entity);

    public virtual void Remove(T entity) => Set.Remove(entity);
}
