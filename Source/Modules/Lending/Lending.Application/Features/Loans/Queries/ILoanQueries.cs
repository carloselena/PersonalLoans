using Lending.Application.Features.Loans.Dtos;
using Lending.Application.Features.Loans.Queries.GetAllLoans;

namespace Lending.Application.Features.Loans.Queries;

public interface ILoanQueries
{
    Task<List<LoanListDto>> GetAllAsync(Guid lenderId, LoanFiltersDto loanFiltersDto, CancellationToken cancellationToken = default);
    Task<LoanDto?> GetByIdAsync(Guid loanId, Guid lenderId, CancellationToken cancellationToken = default);
}