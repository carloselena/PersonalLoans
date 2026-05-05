using Blocks.EntityFramework;
using Lending.Domain;

namespace Lending.Persistence;
public class LendingUnitOfWork(LendingDbContext dbContext)
    : UnitOfWorkEfCore<LendingDbContext>(dbContext), ILendingUnitOfWork;