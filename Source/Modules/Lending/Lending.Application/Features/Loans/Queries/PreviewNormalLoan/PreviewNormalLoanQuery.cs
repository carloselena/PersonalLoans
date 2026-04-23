using Blocks.Domain.Enums;
using Lending.Domain.Loans;
using Lending.Domain.Loans.ValueObjects;
using MediatR;

namespace Lending.Application.Features.Loans.Queries.PreviewNormalLoan;

public record PreviewNormalLoanQuery(
    decimal Principal,
    decimal InterestRate,
    InterestRatePeriod InterestRatePeriod,
    int Term,
    PaymentFrequency PaymentFrequency,
    DateOnly DisbursementDate,
    Currency Currency = Currency.DOP)
    : IRequest<LoanPreviewDto>;