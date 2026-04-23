using Blocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Blocks.EntityFramework;

public abstract class GenericRepository<TAggregate, TContext> : IGenericRepository<TAggregate>
    where TAggregate : class, IAggregateRoot
    where TContext : DbContext
{
    private readonly TContext _dbContext;
    private readonly DbSet<TAggregate> _entity;

    protected GenericRepository(TContext dbContext)
    {
        _dbContext = dbContext;
        _entity = _dbContext.Set<TAggregate>();
    }

    protected TContext Context => _dbContext;

    protected DbSet<TAggregate> Entity => _entity;

    protected virtual IQueryable<TAggregate> Query() => _entity;

    public virtual async Task<TAggregate?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _entity.FindAsync([id], cancellationToken: cancellationToken);

    public virtual async Task<TAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _entity.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public virtual async Task AddAsync(TAggregate aggregateRoot, CancellationToken cancellationToken = default)
        => await _entity.AddAsync(aggregateRoot, cancellationToken);
}