using Accounts.Domain.Lenders;
using Blocks.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Accounts.Persistence.Repositories;

internal class LenderRepository(AccountsDbContext dbContext) :
    AccountsRepository<Lender>(dbContext), ILenderRepository
{
    public async Task<Lender?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Entity
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
    }
}