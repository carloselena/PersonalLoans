using Blocks.Application.Exceptions;
using Blocks.Domain.Time;
using Identity.Application.Features.Auth.Dtos;
using Identity.Application.Options;
using Identity.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    UserManager<User> userManager,
    IJwtService jwtService,
    IOptions<JwtOptions> jwtOptions) : IRequestHandler<RefreshTokenCommand, TokenDto>
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    
    public async Task<TokenDto> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var userId = jwtService.GetUserIdFromExpiredToken(command.AccessToken)
            ?? throw new UnauthorizedException("Token inválido");

        var user = await userManager.Users
                       .Include(u => u.RefreshTokens)
                       .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
                   ?? throw new UnauthorizedException("Usuario no encontrado");

        var tokenHash = TokenFactory.Hash(command.RefreshToken);
        var storedToken = user.RefreshTokens.SingleOrDefault(t => t.TokenHash == tokenHash);

        if (storedToken is null || !storedToken.IsActive(DateProvider.UtcNow()))
            throw new UnauthorizedException("Refresh token inválido o expirado");
        
        user.RevokeRefreshToken(storedToken.TokenHash, DateProvider.UtcNow());

        var tokens = TokenFactory.Issue(user, jwtService, _jwtOptions.RefreshTokenValidForInDays);
        await userManager.UpdateAsync(user);

        return new TokenDto(tokens.AccessToken, tokens.RawRefreshToken);
    }
}