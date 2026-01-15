using Creative.Auth.Application.Persistence;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace Creative.Auth.Api.Endpoints.Auth.Logout;

public record LogoutRequest(string RefreshToken);

public class Logout : Endpoint<LogoutRequest>
{
    private readonly IAuthDbContext _db;

    public Logout(IAuthDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Post("auth/logout");
        AllowAnonymous();
        Tags("Auth");
    }

    public override async Task HandleAsync(LogoutRequest req, CancellationToken ct)
    {
        var refreshToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == req.RefreshToken, ct);

        if (refreshToken != null)
        {
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            await _db.SaveChangesAsync(ct);
        }

        HttpContext.Response.StatusCode = 204;
        Response = null;
    }
}
