using Creative.Auth.Application.Persistence;
using Creative.Auth.Application.Features.Login;
using Creative.Auth.Application.Common.Security;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Creative.Auth.Api.Endpoints.Auth.RefreshToken;

public record RefreshTokenRequest(string Token);

public class RefreshToken : Endpoint<RefreshTokenRequest, AuthResponse>
{
    private readonly IAuthDbContext _db;
    private readonly ITokenGenerator _tokenGenerator;

    public RefreshToken(IAuthDbContext db, ITokenGenerator tokenGenerator)
    {
        _db = db;
        _tokenGenerator = tokenGenerator;
    }

    public override void Configure()
    {
        Post("/auth/refresh-token");
        AllowAnonymous();
        Tags("Auth");
    }

    public override async Task HandleAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == req.Token), ct);

        if (user == null)
            ThrowError("Invalid Token", 401);

        var oldToken = user.RefreshTokens.Single(t => t.Token == req.Token);

        if (!oldToken.IsActive)
            ThrowError("Token is inactive/expired", 401);

        oldToken.Revoked = DateTime.UtcNow;
        oldToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var jwt = _tokenGenerator.GenerateJwtToken(user);
        var newRefreshToken = _tokenGenerator.GenerateRefreshToken();

        user.RefreshTokens.Add(new Creative.Auth.Application.Domain.RefreshToken
        {
            Token = newRefreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            ReplacedByToken = newRefreshToken
        });

        await _db.SaveChangesAsync(ct);

        Response = new AuthResponse(jwt, newRefreshToken, DateTime.UtcNow.AddDays(7), user.Email);
    }
}
