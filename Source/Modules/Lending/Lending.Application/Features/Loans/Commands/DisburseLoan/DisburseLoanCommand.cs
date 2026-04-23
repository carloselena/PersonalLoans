using Lending.Application.Features.Loans.Dtos;
using MediatR;

namespace Lending.Application.Features.Loans.Commands.DisburseLoan;

public record DisburseLoanCommand(Guid LoanId) : IRequest<LoanDto>;