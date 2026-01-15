using Creative.Auth.Application.Persistence;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Creative.Auth.Api.Endpoints.Auth.VerifyEmail;

public class VerifyEmail(IAuthDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("auth/verify-email");
        AllowAnonymous();
        Tags("Auth");
        Summary(s => s.Params["token"] = "The verification token");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var token = Query<string>("token");

        var user = await db.Users.FirstOrDefaultAsync(u => u.VerificationToken == token, ct);

        if (user == null)
            ThrowError("Invalid token", 400);

        user.IsEmailVerified = true;
        user.VerificationToken = null;
        user.VerifiedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        Response = new { Message = "Email verified successfully" };
    }
}
