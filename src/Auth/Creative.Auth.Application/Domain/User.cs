namespace Creative.Auth.Application.Domain;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime? VerifiedAt { get; set; }

    public string? PasswordResetToken { get; set; }
    public DateTime? ResetTokenExpires { get; set; }

    // Relationships
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}