using Blocks.Domain.Abstractions;
using Blocks.Domain.Guards;

namespace Accounts.Domain.Lenders;

public class Lender : AggregateRoot
{
    public Guid UserId { get; private set; }
    
    private Lender() { }

    private Lender(Guid userId)
    {
        UserId = userId;
        Id = Guid.CreateVersion7();
    }

    public static Lender Create(Guid userId)
    {
        Guard.AgainstEmptyGuid(userId, nameof(userId));
        return new Lender(userId);
    }
}