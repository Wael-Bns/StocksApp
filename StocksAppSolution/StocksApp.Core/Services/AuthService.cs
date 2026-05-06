using StocksApp.Core.DTO.AuthenticationDTO;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        public AuthService(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        public async Task<AuthenticationResponse> RegisterAsync(UserAddRequest registerRequest)
        {
            UserResponse userResponse = await _userService.AddUser(registerRequest);
            string jwtToken = _jwtService.CreateJwtToken(userResponse.UserId, userResponse.UserName, userResponse.Email);

            return new AuthenticationResponse
            {
                UserName = userResponse.UserName,
                Email = userResponse.Email,
                Token = jwtToken
            };
        }
    }
}
