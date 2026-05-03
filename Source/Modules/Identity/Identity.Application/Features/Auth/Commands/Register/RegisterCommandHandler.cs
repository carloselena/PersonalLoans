using Blocks.Application.Exceptions;
using Blocks.Domain.Time;
using Blocks.IntegrationEvents;
using Identity.Application.Abstractions;
using Identity.Application.Exceptions;
using Identity.Application.Features.Auth.Dtos;
using Identity.Application.Options;
using Identity.Domain.Users;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Identity.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(
    UserManager<User> userManager,
    IIdentityDbContext dbContext,
    IJwtService jwtService,
    IPublishEndpoint publishEndpoint,
    IOptions<JwtOptions> jwtOptions) : IRequestHandler<RegisterCommand, TokenDto>
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    
    public async Task<TokenDto> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var existing = await userManager.FindByEmailAsync(command.Email);
        if (existing is not null)
            throw new ConflictException("El correo ya está registrado");

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken);
        try
        {
            var user = User.Create(command.Email);

            var result = await userManager.CreateAsync(user, command.Password);
            if (!result.Succeeded)
                throw new IdentityOperationException(result.Errors.Select(e => e.Description));
            
            var tokens = TokenFactory.Issue(user, jwtService, _jwtOptions.RefreshTokenValidForInDays);
            await userManager.UpdateAsync(user);

            var integrationEvent = new UserCreatedIntegrationEvent(
                user.Id,
                DateProvider.UtcNow());
            
            await publishEndpoint.Publish(integrationEvent, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            return new TokenDto(tokens.AccessToken, tokens.RawRefreshToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}