using FluentValidation;
using Lending.Domain.Loans.Enums;

namespace Lending.Application.Features.Loans.Commands.CreateNormalLoan;

public class CreateNormalLoanCommandValidator
    : AbstractValidator<CreateNormalLoanCommand>
{
    public CreateNormalLoanCommandValidator()
    {
        RuleFor(l => l.ClientId)
            .NotEmpty();

        RuleFor(l => l.Principal)
            .GreaterThan(0);

        RuleFor(l => l.Currency)
            .IsInEnum();

        RuleFor(l => l.InterestRate)
            .GreaterThanOrEqualTo(0)
            .LessThan(1);

        RuleFor(l => l.InterestRatePeriod)
            .IsInEnum();

        RuleFor(l => l.PenaltyInterestRate)
            .GreaterThan(0)
            .LessThan(1);

        RuleFor(l => l.PenaltyInterestRatePeriod)
            .IsInEnum();

        RuleFor(l => l.Term)
            .GreaterThan(0);

        RuleFor(l => l.PaymentFrequency)
            .IsInEnum()
            .NotEqual(PaymentFrequency.OneTime);

    }
}