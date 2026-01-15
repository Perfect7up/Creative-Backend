namespace Creative.Auth.Application.Features.Login;


public record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime RefreshTokenExpiration,
    string Email);