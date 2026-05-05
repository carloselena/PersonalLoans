using Lending.Domain.Loans;
using Lending.Domain.Loans.Enums;
using Lending.Persistence.Queries;
using Microsoft.EntityFrameworkCore;

namespace Lending.Persistence.Repositories;

public class LoanRepository(LendingDbContext dbContext)
    : LendingRepository<Loan>(dbContext), ILoanRepository
{
    public async Task<Loan?> GetByIdAsync(Guid id, Guid lenderId, CancellationToken cancellationToken = default)
    {
        return await Entity
            .ForLender(lenderId)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<Loan>> GetOverdueLoansForPenaltyAccrualAsync(DateOnly today, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(l => l.Status == LoanStatus.Active)
            .Where(l => l.Installments.Any(i =>
                i.DueDate < today && i.AmountPaid.Amount < i.Amount.Amount))
            .Include(l => l.Installments)
            .Include(l => l.Penalties)
            .ToListAsync(cancellationToken);
    }
}