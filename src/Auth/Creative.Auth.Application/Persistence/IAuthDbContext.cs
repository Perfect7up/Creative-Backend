using Creative.Auth.Application.Domain;
using Microsoft.EntityFrameworkCore;

namespace Creative.Auth.Application.Persistence;

public interface IAuthDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}