using Creative.Auth.Application.Persistence;
using Creative.Auth.Application.Common.Email;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Creative.Auth.Api.Endpoints.Auth.ForgotPassword;

public record ForgotPasswordRequest(string Email);

public record ForgotPasswordResponse(string Message);

public class ForgotPassword : Endpoint<ForgotPasswordRequest, ForgotPasswordResponse>
{
    private readonly IAuthDbContext _db;
    private readonly EmailSender _emailSender;

    public ForgotPassword(IAuthDbContext db, IEmailSender emailSender)
    {
        _db = db;
        _emailSender = emailSender as EmailSender
                       ?? throw new ArgumentException("EmailSender must be of type EmailSender");
    }

    public override void Configure()
    {
        Post("/auth/forgot-password");
        AllowAnonymous();
        Tags("Auth");
    }

    public override async Task HandleAsync(ForgotPasswordRequest req, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email, ct);

        if (user != null)
        {
            user.PasswordResetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);
            await _db.SaveChangesAsync(ct);
            var resetLink = _emailSender.CreateResetPasswordLink(user.PasswordResetToken);

            await _emailSender.SendEmailAsync(user.Email, "Reset Password",
                $"Click here to reset your password: <a href='{resetLink}'>Reset Password</a>");
        }

        Response = new ForgotPasswordResponse(
            "If an account exists with this email, a reset link has been sent."
        );
    }
}