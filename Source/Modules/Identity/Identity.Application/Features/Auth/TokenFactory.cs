using Blocks.Domain.Time;
using Identity.Application.Options;
using Identity.Domain.Users;
using Microsoft.Extensions.Options;

namespace Identity.Application.Features.Auth;

internal static class TokenFactory
{
    internal static TokenResult Issue(User user, IJwtService jwtService, int refreshTokenValidForInDays)
    {
        var accessToken = jwtService.GenerateAccessToken(user);
        var rawRefreshToken = jwtService.GenerateRefreshToken();
        
        user.AddRefreshToken(
            Hash(rawRefreshToken),
            DateProvider.UtcNow(),
            refreshTokenValidForInDays);
        
        return new TokenResult(accessToken, rawRefreshToken);
    }
    
    internal static string Hash(string token)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
    
    internal sealed record TokenResult(string AccessToken, string RawRefreshToken);
}