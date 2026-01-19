using Creative.Auth.Application.Common.Email;
using Creative.Auth.Application.Domain;
using Creative.Auth.Application.Persistence;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Creative.Auth.Api.Endpoints.Auth.Register;

// Define Request and Response inside the namespace so the Endpoint can find them
public class RegisterRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}

public class RegisterResponse
{
    public string Message { get; set; } = default!;
}

public class Register(IAuthDbContext db, IEmailSender emailSender)
    : Endpoint<RegisterRequest, RegisterResponse>
{
    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();
        Tags("Auth");
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        // 1. Check if user exists
        if (await db.Users.AnyAsync(u => u.Email == req.Email, ct))
            ThrowError("Email already registered", 400);

        // 2. Create the User object
        var verificationToken = Guid.NewGuid().ToString();

        // Ensure you have BCrypt.Net-Next installed via NuGet
        var user = new User
        {
            Email = req.Email,
            FirstName = req.FirstName,
            LastName = req.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            VerificationToken = verificationToken,
            IsEmailVerified = false
        };

        // 3. Save to Database
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        // --- 4. EMAIL LOGIC ---
        var verificationLink = emailSender.CreateVerificationLink(verificationToken);

        var emailBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                <h2 style='color: #00d1ff;'>Confirm your email</h2>
                <p>Hello {user.FirstName},</p>
                <p>Thank you for joining <strong>Creative Hideout</strong>! Please click the button below to verify your email address and activate your account.</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{verificationLink}' style='background-color: #00d1ff; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Confirm Email Address</a>
                </div>
                <p style='font-size: 12px; color: #777;'>If the button doesn't work, copy and paste this link into your browser:</p>
                <p style='font-size: 12px; color: #00d1ff;'>{verificationLink}</p>
            </div>";

        // Sending the HTML Email
        await emailSender.SendEmailAsync(user.Email, "Confirm your Creative Hideout account", emailBody);

        // Sending the raw token Email as requested
        await emailSender.SendEmailAsync(user.Email, "Verify Email", $"Your token is: {user.VerificationToken}");

        // Set the Response object directly as requested
        Response = new RegisterResponse
        {
            Message = "Registration successful. Please check email."
        };
    }
}