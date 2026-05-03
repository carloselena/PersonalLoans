using Blocks.Application.Exceptions;
using Identity.Application.Features.Auth.Dtos;
using Identity.Application.Options;
using Identity.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    UserManager<User> userManager,
    IJwtService jwtService,
    IOptions<JwtOptions> jwtOptions) : IRequestHandler<LoginCommand, TokenDto>
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    
    public async Task<TokenDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
        
        if (user is null || !await userManager.CheckPasswordAsync(user, command.Password))
            throw new UnauthorizedException("Credenciales inválidas");
        
        foreach (var token in user.RefreshTokens.Where(t => t.IsActive(DateTimeOffset.UtcNow)))
            user.RevokeRefreshToken(token.TokenHash, DateTimeOffset.UtcNow);
        
        var tokens = TokenFactory.Issue(user, jwtService, _jwtOptions.RefreshTokenValidForInDays);
        await userManager.UpdateAsync(user);

        return new TokenDto(tokens.AccessToken, tokens.RawRefreshToken);
    }
}