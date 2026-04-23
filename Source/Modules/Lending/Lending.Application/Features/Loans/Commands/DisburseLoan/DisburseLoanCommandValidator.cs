using FluentValidation;

namespace Lending.Application.Features.Loans.Commands.DisburseLoan;

public class DisburseLoanCommandValidator
    :AbstractValidator<DisburseLoanCommand>
{
    public DisburseLoanCommandValidator()
    {
        RuleFor(l => l.LoanId)
            .NotEmpty();
    }
}