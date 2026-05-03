namespace Identity.Application.Options;

public class JwtOptions
{
    public const string SectionName = "JwtOptions";
    
    public string SecretKey { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int AccessTokenValidForInMinutes { get; init; } = 15;
    public int RefreshTokenValidForInDays { get; init; } = 7;
}