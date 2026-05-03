using Blocks.Domain.Exceptions;
using Blocks.Domain.Guards;

namespace Identity.Domain.Users;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    // todo - analyze if necessary: public string? ReplacedByTokenHash { get; private set; }
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsExpired(DateTimeOffset now) => ExpiresAt < now;
    public bool IsActive(DateTimeOffset now) => !IsRevoked && !IsExpired(now);
    
    private RefreshToken() { }
    internal static RefreshToken Create(Guid userId, string tokenHash, DateTimeOffset createdAt, int expirationDays)
    {
        Guard.AgainstEmptyGuid(userId, nameof(userId));
        Guard.AgainstNullOrWhiteSpace(tokenHash, nameof(tokenHash));
        
        return new RefreshToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                CreatedAt = createdAt,
                ExpiresAt = createdAt.AddDays(expirationDays),
                Id = Guid.CreateVersion7()
            };
    }

    internal void Revoke(DateTimeOffset revokedAt)
    {
        if (IsRevoked)
            return;

        RevokedAt = revokedAt;
    }
}