namespace Accounts.Domain.Lenders;

public interface ILenderRepository
{
    Task<Lender?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Lender lender, CancellationToken cancellationToken = default);
}