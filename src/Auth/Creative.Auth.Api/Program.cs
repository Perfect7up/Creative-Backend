using System.Text;
using Creative.Auth.Application.Common.Email;
using Creative.Auth.Application.Common.Security;
using Creative.Auth.Application.Common.Settings;
using Creative.Auth.Application.Persistence;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var authModuleSection = builder.Configuration.GetSection("Modules:Auth");
bool isAuthEnabled = authModuleSection.GetValue<bool>("Enabled");
var frontendSection = authModuleSection.GetSection("Frontend");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IAuthDbContext>(provider =>
    provider.GetRequiredService<AuthDbContext>());
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<ITokenGenerator, TokenGenerator>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.Configure<FrontendSettings>(frontendSection);

builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IAuthDbContext).Assembly);
});

builder.Services.AddFastEndpoints();

if (isAuthEnabled)
{
    builder.Services.SwaggerDocument(o =>
    {
        o.EndpointFilter = e => e.EndpointTags?.Contains("Auth") ?? false;

        o.DocumentSettings = s =>
        {
            s.Title = "Creative Auth API";
            s.DocumentName = "Auth";
            s.Version = "v1";

            s.PostProcess = doc =>
            {
                foreach (var path in doc.Paths.Values)
                {
                    foreach (var operation in path.Values)
                    {
                        operation.Tags.Clear();
                        operation.Tags.Add("Auth");
                    }
                }
            };
        };
    });
}

var jwtSettings = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
if (jwtSettings != null)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });
}
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt")); 

builder.Services.AddFastEndpoints(o =>
{
    o.Assemblies = new[] { typeof(Program).Assembly };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints();

app.Run();