using Identity.Application.Features.Auth.Dtos;
using MediatR;

namespace Identity.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<TokenDto>;