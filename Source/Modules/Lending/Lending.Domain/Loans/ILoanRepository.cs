using Blocks.Domain.Abstractions;

namespace Lending.Domain.Loans;

public interface ILoanRepository : IGenericRepository<Loan>
{
    Task<Loan?> GetByIdAsync(Guid id, Guid lenderId, CancellationToken cancellationToken = default);
    Task<List<Loan>> GetOverdueLoansForPenaltyAccrualAsync(DateOnly today, CancellationToken cancellationToken = default);
}