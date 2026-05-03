using FluentValidation;

namespace Identity.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandValidator
    : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(rt => rt.AccessToken)
            .NotEmpty();

        RuleFor(rt => rt.RefreshToken)
            .NotEmpty();
    }
}