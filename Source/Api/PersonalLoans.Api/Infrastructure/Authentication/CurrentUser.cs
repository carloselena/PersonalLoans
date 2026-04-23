using System.Security.Claims;
using Blocks.Application.Abstractions;

namespace PersonalLoans.Api.Infrastructure.Authentication;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid UserId =>
        GetGuidClaim(ClaimTypes.NameIdentifier);
    
    public Guid LenderId =>
        GetGuidClaim("lender_id");

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    private Guid GetGuidClaim(string type)
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(type);

        if (claim is null)
            throw new UnauthorizedAccessException($"Claim '{type}' no encontrada");

        return !Guid.TryParse(claim.Value, out var guid)
            ? throw new UnauthorizedAccessException($"Claim '{type}' no es un Guid válido") 
            : guid;
    }
}