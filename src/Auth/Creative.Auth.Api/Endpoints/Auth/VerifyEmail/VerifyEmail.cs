using Creative.Auth.Application.Persistence;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Creative.Auth.Api.Endpoints.Auth.VerifyEmail;

public class VerifyEmailRequest
{
    [QueryParam]
    public string Token { get; set; } = default!;
}

public class VerifyEmailResponse
{
    public string Message { get; set; } = default!;
}

public class VerifyEmail(IAuthDbContext db) : Endpoint<VerifyEmailRequest, VerifyEmailResponse>
{
    public override void Configure()
    {
        Get("/auth/verify-email");
        AllowAnonymous();
        Tags("Auth");
    }

    public override async Task HandleAsync(VerifyEmailRequest req, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.VerificationToken == req.Token, ct);

        if (user == null)
            ThrowError("Invalid token", 400);

        user.IsEmailVerified = true;
        user.VerificationToken = null;
        user.VerifiedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        Response = new VerifyEmailResponse
        {
            Message = "Email verified successfully"
        };
    }
}