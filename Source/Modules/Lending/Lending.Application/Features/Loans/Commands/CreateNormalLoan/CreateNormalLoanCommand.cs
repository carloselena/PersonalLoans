using Blocks.Domain.Enums;
using Lending.Domain.Loans.Enums;
using Lending.Domain.Loans.ValueObjects;
using MediatR;

namespace Lending.Application.Features.Loans.Commands.CreateNormalLoan;

public record CreateNormalLoanCommand(
    Guid ClientId,
    decimal Principal,
    decimal InterestRate,
    InterestRatePeriod InterestRatePeriod,
    decimal PenaltyInterestRate,
    InterestRatePeriod PenaltyInterestRatePeriod,
    int Term,
    PaymentFrequency PaymentFrequency,
    Currency Currency = Currency.DOP)
    : IRequest<Guid>;