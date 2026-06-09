using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StocksApp.Core.DTO.AuthenticationDTO;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.Exceptions;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly ITokenService _jwtService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly RefreshTokenOptions _refreshTokenOptions;
        public AuthService(IUserService userService,
            ITokenService jwtService,
            IPasswordHasher passwordHasher,
            IOptions<RefreshTokenOptions> refreshTokenOptions)
        {
            _userService = userService;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _refreshTokenOptions = refreshTokenOptions.Value;
        }

        public async Task<AuthenticationResponse> RegisterAsync(UserAddRequest registerRequest)
        {
            // add the user to the database
            UserResponse userResponse = await _userService.AddUser(registerRequest);

            // generate jwt token and refresh token
            string jwtToken = _jwtService.CreateAccessToken(userResponse.UserId, userResponse.UserName, userResponse.Email);
            string refreshToken = _jwtService.GenerateRefreshToken();
            DateTime refreshTokenExpiry = DateTime.UtcNow.AddMinutes(_refreshTokenOptions.EXPIRATION_MINUTES);

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
        public async Task<AuthenticationResponse> LoginAsync(LoginRequest loginRequest)
        {
            UserResponse? user = await _userService.GetUserByEmail(loginRequest.Email);
            if(user == null)
            {
                throw new InvalidEmailException();
            }
            bool isPasswordValid = _passwordHasher.VerifyPassword(loginRequest.Password, user.PasswordHash);
            if(!isPasswordValid)
            {
                throw new InvalidPasswordException();
            }
            string jwtToken = _jwtService.CreateAccessToken(user.UserId, user.UserName, user.Email);
            string refreshToken = _jwtService.GenerateRefreshToken();
            DateTime refreshTokenExpiry = DateTime.UtcNow.AddMinutes(_refreshTokenOptions.EXPIRATION_MINUTES);

            // update the user refresh token and refresh token expiry in the database
            await _userService.UpdateUserRefreshToken(user.UserId, refreshToken, refreshTokenExpiry);

            return new AuthenticationResponse
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = jwtToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiry = refreshTokenExpiry
            };
        }

        public async Task<AuthenticationResponse> GenerateNewAccessTokenAsync(TokenModel tokenModel)
        {
            if (tokenModel == null || string.IsNullOrEmpty(tokenModel.Token) || string.IsNullOrEmpty(tokenModel.RefreshToken))
            {
                throw new ArgumentException($"There is a missing token in the {nameof(TokenModel)}.");
            }

            ClaimsPrincipal? principal = _jwtService.GetPrincipalFromAccessToken(tokenModel.Token);
            string? email = principal?.FindFirstValue(ClaimTypes.Email);

            if (email == null)
            {
                throw new SecurityTokenException("Invalid access token.");
            }

            UserResponse? userResponse = await _userService.GetUserByEmail(email);

            if (userResponse == null ||
                userResponse.RefreshToken != tokenModel.RefreshToken ||
                userResponse.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            // Generate new tokens
            string newAccessToken = _jwtService.CreateAccessToken(userResponse.UserId, userResponse.UserName, userResponse.Email);
            string newRefreshToken = _jwtService.GenerateRefreshToken();
            
            DateTime newRefreshTokenExpiry = DateTime.UtcNow.AddMinutes(_refreshTokenOptions.EXPIRATION_MINUTES);

            await _userService.UpdateUserRefreshToken(userResponse.UserId, newRefreshToken, newRefreshTokenExpiry);

            return new AuthenticationResponse
            {
                UserName = userResponse.UserName,
                Email = userResponse.Email,
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiry = newRefreshTokenExpiry
            };
        }
    }
}
