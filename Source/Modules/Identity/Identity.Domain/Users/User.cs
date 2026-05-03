using Blocks.Domain.Exceptions;
using Blocks.Domain.Guards;
using Microsoft.AspNetCore.Identity;

namespace Identity.Domain.Users;

public sealed class User : IdentityUser<Guid>
{
    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens;
    
    private User() { }

    private User(string email)
    {
        Email = email.Trim();
        UserName = Email;
        Id = Guid.CreateVersion7();
    }

    public static User Create(string email)
    {
        Guard.AgainstNullOrWhiteSpace(email, nameof(email));
        
        return new User(email);
    }

    public RefreshToken AddRefreshToken(string tokenHash, DateTimeOffset createdAt, int expirationDays)
    {
        var refreshToken = RefreshToken.Create(Id, tokenHash, createdAt, expirationDays);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public void RevokeRefreshToken(string tokenHash, DateTimeOffset revokedAt)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.TokenHash == tokenHash);
        if (refreshToken is null)
            throw new InvalidDomainValueException("No se encontró el RefreshToken");

        refreshToken.Revoke(revokedAt);
    }
}