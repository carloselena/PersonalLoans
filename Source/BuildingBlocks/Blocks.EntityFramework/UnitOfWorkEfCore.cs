using Blocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Blocks.EntityFramework;

public abstract class UnitOfWorkEfCore<TDbContext>(TDbContext dbContext) : IUnitOfWork
    where TDbContext : DbContext
{
    public virtual async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken);
}