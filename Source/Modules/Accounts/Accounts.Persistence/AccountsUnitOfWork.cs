using Accounts.Domain;
using Blocks.EntityFramework;

namespace Accounts.Persistence;

public class AccountsUnitOfWork(AccountsDbContext dbContext)
    : UnitOfWorkEfCore<AccountsDbContext>(dbContext), IAccountsUnitOfWork;