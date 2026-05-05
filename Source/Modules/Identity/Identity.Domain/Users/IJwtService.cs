namespace Identity.Domain.Users;

public interface IJwtService
{
    string GenerateAccessToken(User user, Guid? lenderId = null);
    string GenerateRefreshToken();
    Guid? GetUserIdFromExpiredToken(string token);
}