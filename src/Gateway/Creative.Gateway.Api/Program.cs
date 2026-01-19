using System.Text;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Creative.Auth.Api.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddFastEndpoints(o =>
{
    o.Assemblies = AppDomain.CurrentDomain.GetAssemblies();
});
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<Creative.Auth.Application.Common.Security.JwtOptions>();
if (jwtSettings != null)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(o => {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
            };
        });
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Your Vite Dev URL
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("X-Trace-Id"); // Allow the custom header we created
    });
});
builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.SwaggerDocument(o => {
    o.DocumentSettings = s => { s.Title = "Creative Gateway API"; s.Version = "v1"; };
});

var app = builder.Build();
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(c => {
    c.Endpoints.RoutePrefix = "api";
});

app.UseSwaggerGen();

app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.Run();