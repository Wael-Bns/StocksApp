using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using StocksApp.Core.DTO.AuthenticationDTO;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class AuthServiceTest
    {
        private readonly IAuthService _authService;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly RefreshTokenOptions _refreshTokenOptions;

        public AuthServiceTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _refreshTokenOptions = new RefreshTokenOptions { EXPIRATION_MINUTES = 30 };

            _authService = new AuthService(
                _userServiceMock.Object,
                _tokenServiceMock.Object,
                Mock.Of<IConfiguration>(),
                _passwordHasherMock.Object,
                Options.Create(_refreshTokenOptions));
        }

        #region Register Tests

        [Fact]
        public async Task RegisterAsync_ValidRequest_ReturnsAuthenticationResponse()
        {
            var request = new UserAddRequest { UserName = "TestUser", Email = "test@test.com", Password = "Password123" };
            var userResponse = new UserResponse { UserId = Guid.NewGuid(), UserName = request.UserName!, Email = request.Email! };

            _userServiceMock.Setup(s => s.AddUser(request)).ReturnsAsync(userResponse);
            _tokenServiceMock.Setup(s => s.CreateAccessToken(userResponse.UserId, userResponse.UserName, userResponse.Email))
                .Returns("jwt_token");
            _tokenServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("refresh_token");

            DateTime? capturedExpiry = null;
            _userServiceMock
                .Setup(s => s.UpdateUserRefreshToken(userResponse.UserId, It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback<Guid, string, DateTime>((_, _, expiry) => capturedExpiry = expiry)
                .ReturnsAsync(userResponse);

            AuthenticationResponse response = await _authService.RegisterAsync(request);

            response.Should().NotBeNull();
            response.UserName.Should().Be(userResponse.UserName);
            response.Email.Should().Be(userResponse.Email);
            response.Token.Should().NotBeNullOrWhiteSpace();
            response.RefreshToken.Should().NotBeNullOrWhiteSpace();
            response.RefreshTokenExpiry.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));

            capturedExpiry.Should().NotBeNull();
        }

        #endregion

        #region Login Tests

        [Fact]
        public async Task LoginAsync_InvalidEmail_ThrowsArgumentException()
        {
            var loginRequest = new LoginRequest { Email = "invalid@test.com", Password = "Password123" };
            _userServiceMock.Setup(s => s.GetUserByEmail(loginRequest.Email!)).ReturnsAsync((UserResponse?)null);

            Func<Task> action = async () => await _authService.LoginAsync(loginRequest);

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsArgumentException()
        {
            var loginRequest = new LoginRequest { Email = "test@test.com", Password = "WrongPassword" };
            var userResponse = new UserResponse
            {
                UserId = Guid.NewGuid(),
                UserName = "TestUser",
                Email = loginRequest.Email!,
                PasswordHash = "hash"
            };

            _userServiceMock.Setup(s => s.GetUserByEmail(loginRequest.Email!)).ReturnsAsync(userResponse);
            _passwordHasherMock.Setup(p => p.VerifyPassword(loginRequest.Password!, userResponse.PasswordHash)).Returns(false);

            Func<Task> action = async () => await _authService.LoginAsync(loginRequest);

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthenticationResponse()
        {
            var loginRequest = new LoginRequest { Email = "test@test.com", Password = "Password123" };
            var userResponse = new UserResponse
            {
                UserId = Guid.NewGuid(),
                UserName = "TestUser",
                Email = loginRequest.Email!,
                PasswordHash = "hash"
            };

            _userServiceMock.Setup(s => s.GetUserByEmail(loginRequest.Email!)).ReturnsAsync(userResponse);
            _passwordHasherMock.Setup(p => p.VerifyPassword(loginRequest.Password!, userResponse.PasswordHash)).Returns(true);
            _tokenServiceMock.Setup(s => s.CreateAccessToken(userResponse.UserId, userResponse.UserName, userResponse.Email))
                .Returns("jwt_token");
            _tokenServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("refresh_token");
            _userServiceMock
                .Setup(s => s.UpdateUserRefreshToken(userResponse.UserId, It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(userResponse);

            AuthenticationResponse response = await _authService.LoginAsync(loginRequest);

            response.Should().NotBeNull();
            response.UserName.Should().Be(userResponse.UserName);
            response.Email.Should().Be(userResponse.Email);
            response.Token.Should().NotBeNullOrWhiteSpace();
            response.RefreshToken.Should().NotBeNullOrWhiteSpace();
            response.RefreshTokenExpiry.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
        }

        #endregion

        #region GenerateNewAccessToken Tests

        [Fact]
        public async Task GenerateNewAccessTokenAsync_InvalidRequest_ThrowsArgumentException()
        {
            var tokenModel = new TokenModel { Token = string.Empty, RefreshToken = "refresh" };

            Func<Task> action = async () => await _authService.GenerateNewAccessTokenAsync(tokenModel);

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GenerateNewAccessTokenAsync_InvalidRefreshToken_ThrowsSecurityTokenException()
        {
            var tokenModel = new TokenModel { Token = "token", RefreshToken = "refresh" };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "test@test.com") }));
            var userResponse = new UserResponse
            {
                UserId = Guid.NewGuid(),
                UserName = "TestUser",
                Email = "test@test.com",
                RefreshToken = "different_refresh",
                RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(10)
            };

            _tokenServiceMock.Setup(s => s.GetPrincipalFromAccessToken(tokenModel.Token)).Returns(principal);
            _userServiceMock.Setup(s => s.GetUserByEmail(userResponse.Email)).ReturnsAsync(userResponse);

            Func<Task> action = async () => await _authService.GenerateNewAccessTokenAsync(tokenModel);

            await action.Should().ThrowAsync<SecurityTokenException>();
        }

        [Fact]
        public async Task GenerateNewAccessTokenAsync_ValidRequest_ReturnsAuthenticationResponse()
        {
            var tokenModel = new TokenModel { Token = "token", RefreshToken = "refresh" };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "test@test.com") }));
            var userResponse = new UserResponse
            {
                UserId = Guid.NewGuid(),
                UserName = "TestUser",
                Email = "test@test.com",
                RefreshToken = "refresh",
                RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(10)
            };

            _tokenServiceMock.Setup(s => s.GetPrincipalFromAccessToken(tokenModel.Token)).Returns(principal);
            _userServiceMock.Setup(s => s.GetUserByEmail(userResponse.Email)).ReturnsAsync(userResponse);
            _tokenServiceMock.Setup(s => s.CreateAccessToken(userResponse.UserId, userResponse.UserName, userResponse.Email))
                .Returns("new_access");
            _tokenServiceMock.Setup(s => s.GenerateRefreshToken()).Returns("new_refresh");
            _userServiceMock
                .Setup(s => s.UpdateUserRefreshToken(userResponse.UserId, It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(userResponse);

            AuthenticationResponse response = await _authService.GenerateNewAccessTokenAsync(tokenModel);

            response.Should().NotBeNull();
            response.UserName.Should().Be(userResponse.UserName);
            response.Email.Should().Be(userResponse.Email);
            response.Token.Should().NotBeNullOrWhiteSpace();
            response.RefreshToken.Should().NotBeNullOrWhiteSpace();
            response.RefreshTokenExpiry.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
        }

        #endregion
    }
}