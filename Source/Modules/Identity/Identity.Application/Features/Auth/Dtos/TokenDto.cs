namespace Identity.Application.Features.Auth.Dtos;

public record TokenDto(string AccessToken, string RefreshToken);