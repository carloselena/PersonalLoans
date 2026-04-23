using FluentValidation;

namespace Lending.Application.Features.Loans.Queries.GetLoanById;

public class GetLoanByIdQueryValidator
    : AbstractValidator<GetLoanByIdQuery>
{
    public GetLoanByIdQueryValidator()
    {
        RuleFor(l => l.LoanId)
            .NotEmpty();
    }
}