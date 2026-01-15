using Creative.Auth.Application.Domain;

namespace Creative.Auth.Application.Common.Security;

public interface ITokenGenerator
{
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
}