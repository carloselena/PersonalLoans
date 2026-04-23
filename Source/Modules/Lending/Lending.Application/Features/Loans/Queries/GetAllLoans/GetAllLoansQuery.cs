using Lending.Domain.Loans;
using MediatR;

namespace Lending.Application.Features.Loans.Queries.GetAllLoans;

public record GetAllLoansQuery : LoanFiltersDto, IRequest<List<LoanListDto>>;