using Creative.Auth.Application.Persistence;
using Creative.Auth.Application.Features.Login;
using Creative.Auth.Application.Common.Security;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace Creative.Auth.Api.Endpoints.Auth.Login;

public class Login : Endpoint<LoginRequest, AuthResponse>
{
    private readonly IAuthDbContext _db;
    private readonly ITokenGenerator _tokenGenerator;

    public Login(IAuthDbContext db, ITokenGenerator tokenGenerator)
    {
        _db = db;
        _tokenGenerator = tokenGenerator;
    }

    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
        Tags("Auth");

    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email, ct);

        if (user == null)
            ThrowError("No account found with this email. Please sign up first.", 401);

        if (!user.IsEmailVerified)
            ThrowError("Your email is not verified. Please check your inbox for the confirmation link.", 403);

        if (!BC.Verify(req.Password, user.PasswordHash))
            ThrowError("The password you entered is incorrect.", 401);

        var jwt = _tokenGenerator.GenerateJwtToken(user);
        var refresh = _tokenGenerator.GenerateRefreshToken();

        user.RefreshTokens.Add(new Creative.Auth.Application.Domain.RefreshToken
        {
            Token = refresh,
            Expires = DateTime.UtcNow.AddDays(7),
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        });

        await _db.SaveChangesAsync(ct);

        Response = new AuthResponse(jwt, refresh, DateTime.UtcNow.AddDays(7), user.Email);
    }
}
