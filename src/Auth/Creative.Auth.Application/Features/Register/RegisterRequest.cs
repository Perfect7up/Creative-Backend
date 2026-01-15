namespace Creative.Auth.Application.Features.Register;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);