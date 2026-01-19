using Creative.Auth.Application.Domain;
using Creative.Auth.Application.Persistence;
using Creative.Auth.Application.Features.Register;
using Creative.Auth.Application.Common.Email;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace Creative.Auth.Api.Endpoints.Auth.Register;

public class Register(IAuthDbContext db, IEmailSender emailSender)
    : Endpoint<RegisterRequest>
{
    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();
        Tags("Auth");
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Email == req.Email, ct))
        {
            ThrowError("Email already exists", 400);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = req.Email,
            FirstName = req.FirstName,
            LastName = req.LastName,
            PasswordHash = BC.HashPassword(req.Password),
            VerificationToken = Guid.NewGuid().ToString()
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        await emailSender.SendEmailAsync(user.Email, "Verify Email",
            $"Your token is: {user.VerificationToken}");
        Response = new { Message = "Registration successful. Please check email." };
    }
}
