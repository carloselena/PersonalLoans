using FluentValidation;

namespace Lending.Application.Features.Loans.Queries.GetAllLoans;

public class GetAllLoansQueryValidator
    : AbstractValidator<GetAllLoansQuery>
{
    public GetAllLoansQueryValidator()
    {
        RuleFor(l => l.ClientId)
            .NotEmpty()
            .When(l => l.ClientId.HasValue);

        RuleFor(l => l.PaymentFrequency)
            .IsInEnum()
            .When(l => l.PaymentFrequency.HasValue);
        
        RuleFor(l => l.Status)
            .IsInEnum()
            .When(l => l.Status.HasValue);
        
        RuleFor(l => l.MinPrincipal)
            .GreaterThanOrEqualTo(0)
            .When(l => l.MinPrincipal.HasValue);
        
        RuleFor(l => l.MaxPrincipal)
            .GreaterThanOrEqualTo(0)
            .When(l => l.MaxPrincipal.HasValue);
        
        RuleFor(l => l)
            .Must(l => !l.MinPrincipal.HasValue || !l.MaxPrincipal.HasValue || l.MinPrincipal <= l.MaxPrincipal);

        RuleFor(l => l.MinTerm)
            .GreaterThan(0)
            .When(l => l.MinTerm.HasValue);
        
        RuleFor(l => l.MaxTerm)
            .GreaterThan(0)
            .When(l => l.MaxTerm.HasValue);

        RuleFor(l => l)
            .Must(l => !l.MinTerm.HasValue || !l.MaxTerm.HasValue || l.MinTerm <= l.MaxTerm);
    }
}