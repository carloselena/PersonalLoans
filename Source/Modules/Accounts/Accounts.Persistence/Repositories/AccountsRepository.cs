using Blocks.Domain.Abstractions;
using Blocks.EntityFramework;

namespace Accounts.Persistence.Repositories;

public class AccountsRepository<TAggregate>(AccountsDbContext dbContext)
    : GenericRepository<TAggregate, AccountsDbContext>(dbContext)
    where TAggregate : class, IAggregateRoot;