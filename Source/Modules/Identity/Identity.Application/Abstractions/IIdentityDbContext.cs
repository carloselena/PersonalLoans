using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Identity.Application.Abstractions;

public interface IIdentityDbContext
{
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}