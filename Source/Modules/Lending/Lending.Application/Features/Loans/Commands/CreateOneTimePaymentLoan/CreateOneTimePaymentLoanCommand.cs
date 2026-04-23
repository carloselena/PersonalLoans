using Blocks.Domain.Enums;
using Lending.Domain.Loans.ValueObjects;
using MediatR;

namespace Lending.Application.Features.Loans.Commands.CreateOneTimePaymentLoan;

public record CreateOneTimePaymentLoanCommand(
    Guid ClientId,
    decimal Principal,
    Currency Currency,
    decimal Interest,
    decimal PenaltyInterestRate,
    InterestRatePeriod PenaltyInterestRatePeriod,
    DateOnly DueDate)
    : IRequest<Guid>;