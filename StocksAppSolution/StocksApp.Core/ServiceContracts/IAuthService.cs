using StocksApp.Core.DTO.AuthenticationDTO;
using StocksApp.Core.DTO.UsersDTO;

namespace StocksApp.Core.ServiceContracts
{
    public interface IAuthService
    {
        Task<AuthenticationResponse> RegisterAsync(UserAddRequest registerRequest);
        Task<AuthenticationResponse> LoginAsync(LoginRequest loginRequest);
        Task<AuthenticationResponse> GenerateNewAccessTokenAsync(TokenModel tokenModel);
    }
}