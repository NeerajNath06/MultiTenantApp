using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user, Tenant tenant, List<string> roles, List<string> permissions);
    string GenerateRefreshToken();
    System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
