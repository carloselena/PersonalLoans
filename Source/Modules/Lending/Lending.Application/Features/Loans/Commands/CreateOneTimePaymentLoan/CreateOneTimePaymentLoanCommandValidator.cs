using FluentValidation;

namespace Lending.Application.Features.Loans.Commands.CreateOneTimePaymentLoan;

public class CreateOneTimePaymentLoanCommandValidator
    : AbstractValidator<CreateOneTimePaymentLoanCommand>
{
    public CreateOneTimePaymentLoanCommandValidator()
    {
        RuleFor(l => l.ClientId)
            .NotEmpty();

        RuleFor(l => l.Principal)
            .GreaterThan(0);

        RuleFor(l => l.Currency)
            .IsInEnum();

        RuleFor(l => l.Interest)
            .GreaterThan(0);

        RuleFor(l => l.PenaltyInterestRate)
            .GreaterThan(0)
            .LessThan(1);

        RuleFor(l => l.PenaltyInterestRatePeriod)
            .IsInEnum();

        RuleFor(l => l.DueDate)
            .GreaterThan(DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date));
    }
}