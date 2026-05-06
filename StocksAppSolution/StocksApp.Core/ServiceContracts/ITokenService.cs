using System.Security.Claims;

namespace StocksApp.Core.ServiceContracts
{
    public interface ITokenService
    {
        string CreateAccessToken(Guid userId, string username, string email);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromAccessToken(string? token);
    }
}