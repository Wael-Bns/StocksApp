namespace StocksApp.Core.ServiceContracts
{
    public interface IJwtService
    {
        string CreateJwtToken(Guid userId, string username, string email);
        public string GenerateRefreshToken();
    }
}