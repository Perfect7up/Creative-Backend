using Creative.Auth.Application.Persistence;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace Creative.Auth.Api.Endpoints.Auth.ResetPassword;

public record ResetPasswordRequest(string Token, string NewPassword);

public class ResetPassword(IAuthDbContext db) : Endpoint<ResetPasswordRequest>
{
    public override void Configure()
    {
        Post("/auth/reset-password");
        AllowAnonymous();
        Tags("Auth");
    }

    public override async Task HandleAsync(ResetPasswordRequest req, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.PasswordResetToken == req.Token &&
            u.ResetTokenExpires > DateTime.UtcNow, ct);

        if (user == null)
            ThrowError("Invalid or expired reset token", 400);

        user.PasswordHash = BC.HashPassword(req.NewPassword);
        user.PasswordResetToken = null;
        user.ResetTokenExpires = null;

        await db.SaveChangesAsync(ct);

        Response = new { Message = "Password has been reset successfully. You can now login." };
    }
}
