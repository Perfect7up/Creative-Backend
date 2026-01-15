// The namespace now includes ".DependencyInjection"
namespace Creative.Auth.Api.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Creative.Auth.Application.Persistence;
using Creative.Auth.Application.Common.Email;
using Creative.Auth.Application.Common.Security;
using Creative.Auth.Application.Common.Settings;
using Microsoft.EntityFrameworkCore;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration config)
    {
        var authModuleSection = config.GetSection("Modules:Auth");
        var connectionString = config.GetConnectionString("DefaultConnection");

        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAuthDbContext>(provider =>
            provider.GetRequiredService<AuthDbContext>());

        services.Configure<JwtOptions>(config.GetSection("Jwt"));
        services.AddSingleton<ITokenGenerator, TokenGenerator>();

        services.Configure<EmailSettings>(config.GetSection("Email"));
        services.Configure<FrontendSettings>(authModuleSection.GetSection("Frontend"));
        services.AddScoped<IEmailSender, EmailSender>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(IAuthDbContext).Assembly));

        return services;
    }
}