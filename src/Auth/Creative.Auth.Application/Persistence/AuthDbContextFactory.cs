using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Creative.Auth.Application.Persistence;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets("cf4bbc17-9f0d-43fa-991f-de46130542cc")
            .Build();

        var builder = new DbContextOptionsBuilder<AuthDbContext>();
        var connectionString = "Host=localhost;Database=Creative;Username=postgres;Password=mansoor992006";

        builder.UseNpgsql(connectionString);

        return new AuthDbContext(builder.Options);
    }
}