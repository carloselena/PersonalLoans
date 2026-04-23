using Lending.Domain.Loans;

namespace Lending.Application.Features.Loans.Queries.GetAllLoans;

public record LoanFiltersDto
{
    public Guid? ClientId { get; init; }
    public bool? IsOverDue { get; init; }
    public bool? IsDisbursed { get; init; }
    public PaymentFrequency? PaymentFrequency { get; init; }
    public LoanStatus? Status { get; init; }
    public decimal? MinPrincipal { get; init; }
    public decimal? MaxPrincipal { get; init; }
    public int? MinTerm { get; init; }
    public int? MaxTerm { get; init; }
}