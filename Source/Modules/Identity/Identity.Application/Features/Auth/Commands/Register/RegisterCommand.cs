using Identity.Application.Features.Auth.Dtos;
using MediatR;

namespace Identity.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password) : IRequest<TokenDto>;