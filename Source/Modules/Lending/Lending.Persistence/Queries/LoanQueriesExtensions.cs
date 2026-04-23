using Lending.Domain.Loans;

namespace Lending.Persistence.Queries;

public static class LoanQueriesExtensions
{
    public static IQueryable<Loan> ForLender(
        this IQueryable<Loan> query,
        Guid lenderId)
        => query.Where(l => l.LenderId == lenderId);
}