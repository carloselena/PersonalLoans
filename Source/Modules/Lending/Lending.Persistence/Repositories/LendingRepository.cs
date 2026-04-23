using Blocks.Domain.Abstractions;
using Blocks.EntityFramework;

namespace Lending.Persistence.Repositories;

public class LendingRepository<TAggregate>(LendingDbContext dbContext)
    : GenericRepository<TAggregate, LendingDbContext>(dbContext)
     where TAggregate : class, IAggregateRoot;