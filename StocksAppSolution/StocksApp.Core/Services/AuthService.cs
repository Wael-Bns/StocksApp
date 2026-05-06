using Microsoft.Extensions.Configuration;
using StocksApp.Core.DTO.AuthenticationDTO;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        public AuthService(IUserService userService, IJwtService jwtService, IConfiguration configuration)
        {
            _userService = userService;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        public async Task<AuthenticationResponse> RegisterAsync(UserAddRequest registerRequest)
        {
            // add the user to the database
            UserResponse userResponse = await _userService.AddUser(registerRequest);

            // generate jwt token and refresh token
            string jwtToken = _jwtService.CreateJwtToken(userResponse.UserId, userResponse.UserName, userResponse.Email);
            string refreshToken = _jwtService.GenerateRefreshToken();
            DateTime refreshTokenExpiry = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["RefreshToken:EXPIRATION_MINUTES"]));

            // update the user refresh token and refresh token expiry in the database
            await _userService.UpdateUserRefreshToken(userResponse.UserId, refreshToken, refreshTokenExpiry);
            
            return new AuthenticationResponse
            {
                UserName = userResponse.UserName,
                Email = userResponse.Email,
                Token = jwtToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiry = refreshTokenExpiry
            };
        }
    }
}
