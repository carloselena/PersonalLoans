using Blocks.EntityFramework;

namespace Lending.Persistence;

public class LendingUnitOfWork(LendingDbContext dbContext)
    : UnitOfWorkEfCore<LendingDbContext>(dbContext);