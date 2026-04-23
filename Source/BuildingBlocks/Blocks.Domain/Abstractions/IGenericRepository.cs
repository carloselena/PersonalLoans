namespace Blocks.Domain.Abstractions;

public interface IGenericRepository<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
{
    public Task<TAggregateRoot?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<TAggregateRoot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task AddAsync(TAggregateRoot aggregateRoot, CancellationToken cancellationToken = default);
}