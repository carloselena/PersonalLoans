using Blocks.Domain.Time;
using Lending.Application.Features.Loans.Dtos;
using Lending.Application.Features.Loans.Queries;
using Lending.Application.Features.Loans.Queries.GetAllLoans;
using Lending.Domain.Loans;
using Lending.Domain.Loans.Installments;
using Microsoft.EntityFrameworkCore;

namespace Lending.Persistence.Queries;

public class LoanQueries(LendingDbContext dbContext) : ILoanQueries
{
    public async Task<List<LoanListDto>> GetAllAsync(Guid lenderId, LoanFiltersDto loanFiltersDto, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Loans
            .AsNoTracking()
            .ForLender(lenderId);

        var today = DateProvider.Today();

        query = ApplyFilters(query, loanFiltersDto, today);
        
        return await query
            .Select(l => new LoanListDto(
                l.Id,
                l.ClientId,
                l.LenderId,
                l.PaymentFrequency,
                (decimal)l.Principal,
                l.Principal.Currency,
                l.InterestRate.Rate,
                l.InterestRate.Period,
                l.PenaltyInterestRate.Rate,
                l.PenaltyInterestRate.Period,
                l.Term.Value,
                l.Installments.OrderBy(i => i.Number).Select(i => i.DueDate).FirstOrDefault(),
                l.Installments.OrderByDescending(i => i.Number).Select(i => i.DueDate).FirstOrDefault(),
                l.Installments.Count(i => i.Status == InstallmentStatus.Paid),
                l.Installments.Count(i => i.Status == InstallmentStatus.Pending),
                l.Installments.Sum(i => (decimal?)i.Amount) ?? 0,
                l.Installments.Sum(i => (decimal?)i.AmountPaid) ?? 0,
                l.Installments.Sum(i => (decimal?)i.AmountOwed) ?? 0,
                l.Penalties.Sum(p => (decimal?)p.Amount) ?? 0,
                l.Penalties.Sum(p => (decimal?)p.AmountOwed) ?? 0,
                l.DisbursedAt.HasValue,
                l.CreatedAt,
                l.DisbursedAt
            )).ToListAsync(cancellationToken);
    }

    public async Task<LoanDto?> GetByIdAsync(Guid loanId, Guid lenderId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Loans
            .AsNoTracking()
            .ForLender(lenderId)
            .Where(l => l.Id == loanId)
            .Select(l => new LoanDto(
                l.Id,
                l.LenderId,
                l.ClientId,
                (decimal)l.Principal,
                l.Principal.Currency,
                l.InterestRate.Rate,
                l.InterestRate.Period,
                l.PenaltyInterestRate.Rate,
                l.PenaltyInterestRate.Period,
                l.Term.Value,
                l.PaymentFrequency,
                l.Status,
                l.OneTimeInterest == null ? null : (decimal)l.OneTimeInterest.Amount,
                l.CreatedAt,
                l.DisbursedAt,
                l.Installments
                    .OrderBy(i => i.Number)
                    .Select(i => new InstallmentDto(
                            i.Number,
                            i.DueDate,
                            (decimal)i.Amount,
                            (decimal)i.Principal,
                            (decimal)i.Interest,
                            (decimal)i.AmountPaid,
                            (decimal)i.AmountOwed,
                            i.Amount.Currency,
                            i.Status
                        )
                    )
                    .ToList(),
                l.Penalties
                    .OrderBy(p => p.InstallmentNumber)
                    .ThenBy(p => p.AppliedAt)
                    .Select(p => new PenaltyDto(
                        p.Id,
                        p.InstallmentNumber,
                        (decimal)p.Amount,
                        (decimal)p.AmountPaid,
                        (decimal)p.AmountOwed,
                        p.Amount.Currency,
                        p.AppliedAt,
                        p.LastAppliedAt,
                        p.DaysAccrued,
                        p.IsPaid))
                    .ToList()

            )).FirstOrDefaultAsync(cancellationToken);
    }
    
    private static IQueryable<Loan> ApplyFilters(IQueryable<Loan> query, LoanFiltersDto filters, DateOnly today)
    {
        if (filters.ClientId.HasValue)
            query = query.Where(l => l.ClientId == filters.ClientId.Value);
        
        if (filters.PaymentFrequency.HasValue)
            query = query.Where(l => l.PaymentFrequency == filters.PaymentFrequency.Value);
        
        if (filters.Status.HasValue)
            query = query.Where(l => l.Status == filters.Status.Value);
        
        if (filters.MinPrincipal.HasValue)
            query = query.Where(l => l.Principal.Amount >= filters.MinPrincipal.Value);

        if (filters.MaxPrincipal.HasValue)
            query = query.Where(l => l.Principal.Amount <= filters.MaxPrincipal.Value);
        
        if (filters.MinTerm.HasValue)
            query = query.Where(l => l.Term.Value >= filters.MinTerm.Value);
        
        if (filters.MaxTerm.HasValue)
            query = query.Where(l => l.Term.Value <= filters.MaxTerm.Value);

        if (filters.IsDisbursed.HasValue)
        {
            query = filters.IsDisbursed.Value
                ? query.Where(l => l.DisbursedAt != null)
                : query.Where(l => l.DisbursedAt == null);
        }

        if (filters.IsOverDue.HasValue)
        {
            query = filters.IsOverDue.Value
                ? query.Where(l => l.Installments.Any(i => i.DueDate < today && i.Status == InstallmentStatus.Pending))
                : query.Where(l => !l.Installments.Any(i => i.DueDate < today && i.Status == InstallmentStatus.Pending));
        }

        return query;
    }
}