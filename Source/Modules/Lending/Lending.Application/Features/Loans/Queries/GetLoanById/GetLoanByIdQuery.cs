using Lending.Application.Features.Loans.Dtos;
using MediatR;

namespace Lending.Application.Features.Loans.Queries.GetLoanById;

public record GetLoanByIdQuery(Guid LoanId) : IRequest<LoanDto>;