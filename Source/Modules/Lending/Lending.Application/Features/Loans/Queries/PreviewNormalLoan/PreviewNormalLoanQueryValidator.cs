using FluentValidation;
using Lending.Domain.Loans;

namespace Lending.Application.Features.Loans.Queries.PreviewNormalLoan;

public class PreviewNormalLoanQueryValidator
    : AbstractValidator<PreviewNormalLoanQuery>
{
    public PreviewNormalLoanQueryValidator()
    {
        RuleFor(l => l.Principal)
            .GreaterThan(0);

        RuleFor(l => l.InterestRate)
            .GreaterThanOrEqualTo(0)
            .LessThan(1);

        RuleFor(l => l.InterestRatePeriod)
            .IsInEnum();

        RuleFor(l => l.Term)
            .GreaterThan(0);

        RuleFor(l => l.PaymentFrequency)
            .IsInEnum()
            .NotEqual(PaymentFrequency.OneTime);

        RuleFor(l => l.DisbursementDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date));
    }
}