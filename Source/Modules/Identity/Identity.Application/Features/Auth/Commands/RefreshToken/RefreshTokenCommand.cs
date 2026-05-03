using Identity.Application.Features.Auth.Dtos;
using MediatR;

namespace Identity.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<TokenDto>;