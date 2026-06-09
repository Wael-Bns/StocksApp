using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using StocksApp.Core.DTO.AuthenticationDTO;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.Exceptions;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class AuthServiceTest
    {
        private const string TestEmail = "test@test.com";
        private const string TestUserName = "TestUser";
        private const string TestPassword = "Password123";
        private const string WrongPassword = "WrongPassword";
        private const string PasswordHash = "password_hash";
        private const string AccessToken = "access_token";
        private const string RefreshToken = "refresh_token";
        private const string NewAccessToken = "new_access_token";
        private const string NewRefreshToken = "new_refresh_token";
        private const string DifferentRefreshToken = "different_refresh_token";
        private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

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
                _passwordHasherMock.Object,
                Options.Create(_refreshTokenOptions));
        }

        #region Register Tests

        [Fact]
        public async Task RegisterAsync_ValidRequest_ReturnsAuthenticationResponse()
        {
            var request = new UserAddRequest { UserName = TestUserName, Email = TestEmail, Password = TestPassword };
            var userResponse = new UserResponse { UserId = TestUserId, UserName = request.UserName!, Email = request.Email! };

            _userServiceMock.Setup(s => s.AddUser(request)).ReturnsAsync(userResponse);
            _tokenServiceMock.Setup(s => s.CreateAccessToken(userResponse.UserId, userResponse.UserName, userResponse.Email))
                .Returns(AccessToken);
            _tokenServiceMock.Setup(s => s.GenerateRefreshToken()).Returns(RefreshToken);

            _userServiceMock
                .Setup(s => s.UpdateUserRefreshToken(userResponse.UserId, It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(userResponse);

            AuthenticationResponse response = await _authService.RegisterAsync(request);

            response.Should().NotBeNull();
            response.UserName.Should().Be(userResponse.UserName);
            response.Email.Should().Be(userResponse.Email);
            response.Token.Should().Be(AccessToken);
            response.RefreshToken.Should().Be(RefreshToken);

            _userServiceMock.Verify(
                s => s.UpdateUserRefreshToken(userResponse.UserId, RefreshToken, It.IsAny<DateTime>()),
                Times.Once);
        }

        #endregion

        #region Login Tests

        [Fact]
        public async Task LoginAsync_InvalidEmail_ThrowsClientException()
        {
            var loginRequest = new LoginRequest { Email = "invalid@test.com", Password = TestPassword };
            _userServiceMock.Setup(s => s.GetUserByEmail(loginRequest.Email!)).ReturnsAsync((UserResponse?)null);

            Func<Task> action = async () => await _authService.LoginAsync(loginRequest);

            await action.Should().ThrowAsync<ClientException>();
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsClientException()
        {
            var loginRequest = new LoginRequest { Email = TestEmail, Password = WrongPassword };
            var userResponse = new UserResponse
            {
                UserId = TestUserId,
                UserName = TestUserName,
                Email = loginRequest.Email!,
                PasswordHash = PasswordHash
            };

            _userServiceMock.Setup(s => s.GetUserByEmail(loginRequest.Email!)).ReturnsAsync(userResponse);
            _passwordHasherMock.Setup(p => p.VerifyPassword(loginRequest.Password!, userResponse.PasswordHash)).Returns(false);

            Func<Task> action = async () => await _authService.LoginAsync(loginRequest);

            await action.Should().ThrowAsync<ClientException>();
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthenticationResponse()
        {
            var loginRequest = new LoginRequest { Email = TestEmail, Password = TestPassword };
            var userResponse = new UserResponse
            {
                UserId = TestUserId,
                UserName = TestUserName,
                Email = loginRequest.Email!,
                PasswordHash = PasswordHash
            };

            _userServiceMock.Setup(s => s.GetUserByEmail(loginRequest.Email!)).ReturnsAsync(userResponse);
            _passwordHasherMock.Setup(p => p.VerifyPassword(loginRequest.Password!, userResponse.PasswordHash)).Returns(true);
            _tokenServiceMock.Setup(s => s.CreateAccessToken(userResponse.UserId, userResponse.UserName, userResponse.Email))
                .Returns(AccessToken);
            _tokenServiceMock.Setup(s => s.GenerateRefreshToken()).Returns(RefreshToken);
            _userServiceMock
                .Setup(s => s.UpdateUserRefreshToken(userResponse.UserId, It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(userResponse);

            AuthenticationResponse response = await _authService.LoginAsync(loginRequest);

            response.Should().NotBeNull();
            response.UserName.Should().Be(userResponse.UserName);
            response.Email.Should().Be(userResponse.Email);
            response.Token.Should().Be(AccessToken);
            response.RefreshToken.Should().Be(RefreshToken);
            _userServiceMock.Verify(
                s => s.UpdateUserRefreshToken(userResponse.UserId, RefreshToken, It.IsAny<DateTime>()),
                Times.Once);
        }

        #endregion

        #region GenerateNewAccessToken Tests

        [Fact]
        public async Task GenerateNewAccessTokenAsync_InvalidRequest_ThrowsArgumentException()
        {
            var tokenModel = new TokenModel { Token = string.Empty, RefreshToken = RefreshToken };

            Func<Task> action = async () => await _authService.GenerateNewAccessTokenAsync(tokenModel);

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GenerateNewAccessTokenAsync_InvalidAccessToken_ThrowsSecurityTokenException()
        {
            var tokenModel = CreateTokenModel();

            _tokenServiceMock.Setup(s => s.GetPrincipalFromAccessToken(tokenModel.Token)).Returns((ClaimsPrincipal?)null);

            Func<Task> action = async () => await _authService.GenerateNewAccessTokenAsync(tokenModel);

            await action.Should().ThrowAsync<SecurityTokenException>();
        }

        [Fact]
        public async Task GenerateNewAccessTokenAsync_AccessTokenWithoutEmail_ThrowsSecurityTokenException()
        {
            var tokenModel = CreateTokenModel();
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, TestUserName) }));

            _tokenServiceMock.Setup(s => s.GetPrincipalFromAccessToken(tokenModel.Token)).Returns(principal);

            Func<Task> action = async () => await _authService.GenerateNewAccessTokenAsync(tokenModel);

            await action.Should().ThrowAsync<SecurityTokenException>();
        }

        [Fact]
        public async Task GenerateNewAccessTokenAsync_UserNotFound_ThrowsSecurityTokenException()
        {
            var tokenModel = CreateTokenModel();
            var principal = CreatePrincipal(TestEmail);

            _tokenServiceMock.Setup(s => s.GetPrincipalFromAccessToken(tokenModel.Token)).Returns(principal);
            _userServiceMock.Setup(s => s.GetUserByEmail(TestEmail)).ReturnsAsync((UserResponse?)null);

            Func<Task> action = async () => await _authService.GenerateNewAccessTokenAsync(tokenModel);

            await action.Should().ThrowAsync<SecurityTokenException>();
        }

        [Fact]
        public async Task GenerateNewAccessTokenAsync_InvalidRefreshToken_ThrowsSecurityTokenException()
        {
            var tokenModel = CreateTokenModel();
            var principal = CreatePrincipal(TestEmail);
            var userResponse = new UserResponse
            {
                UserId = TestUserId,
                UserName = TestUserName,
                Email = TestEmail,
                RefreshToken = DifferentRefreshToken,
                RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(10)
            };

            _tokenServiceMock.Setup(s => s.GetPrincipalFromAccessToken(tokenModel.Token)).Returns(principal);
            _userServiceMock.Setup(s => s.GetUserByEmail(userResponse.Email)).ReturnsAsync(userResponse);

            Func<Task> action = async () => await _authService.GenerateNewAccessTokenAsync(tokenModel);

            await action.Should().ThrowAsync<SecurityTokenException>();
        }

        [Fact]
        public async Task GenerateNewAccessTokenAsync_ExpiredRefreshToken_ThrowsSecurityTokenException()
        {
            var tokenModel = CreateTokenModel();
            var principal = CreatePrincipal(TestEmail);
            var userResponse = new UserResponse
            {
                UserId = TestUserId,
                UserName = TestUserName,
                Email = TestEmail,
                RefreshToken = tokenModel.RefreshToken,
                RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(-1)
            };

            _tokenServiceMock.Setup(s => s.GetPrincipalFromAccessToken(tokenModel.Token)).Returns(principal);
            _userServiceMock.Setup(s => s.GetUserByEmail(userResponse.Email)).ReturnsAsync(userResponse);

            Func<Task> action = async () => await _authService.GenerateNewAccessTokenAsync(tokenModel);

            await action.Should().ThrowAsync<SecurityTokenException>();
        }

        [Fact]
        public async Task GenerateNewAccessTokenAsync_ValidRequest_ReturnsAuthenticationResponse()
        {
            var tokenModel = CreateTokenModel();
            var principal = CreatePrincipal(TestEmail);
            var userResponse = new UserResponse
            {
                UserId = TestUserId,
                UserName = TestUserName,
                Email = TestEmail,
                RefreshToken = tokenModel.RefreshToken,
                RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(10)
            };

            _tokenServiceMock.Setup(s => s.GetPrincipalFromAccessToken(tokenModel.Token)).Returns(principal);
            _userServiceMock.Setup(s => s.GetUserByEmail(userResponse.Email)).ReturnsAsync(userResponse);
            _tokenServiceMock.Setup(s => s.CreateAccessToken(userResponse.UserId, userResponse.UserName, userResponse.Email))
                .Returns(NewAccessToken);
            _tokenServiceMock.Setup(s => s.GenerateRefreshToken()).Returns(NewRefreshToken);
            _userServiceMock
                .Setup(s => s.UpdateUserRefreshToken(userResponse.UserId, It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(userResponse);

            AuthenticationResponse response = await _authService.GenerateNewAccessTokenAsync(tokenModel);

            response.Should().NotBeNull();
            response.UserName.Should().Be(userResponse.UserName);
            response.Email.Should().Be(userResponse.Email);
            response.Token.Should().Be(NewAccessToken);
            response.RefreshToken.Should().Be(NewRefreshToken);
            response.RefreshTokenExpiry.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
            _userServiceMock.Verify(
                s => s.UpdateUserRefreshToken(userResponse.UserId, NewRefreshToken, It.IsAny<DateTime>()),
                Times.Once);
        }

        private static TokenModel CreateTokenModel()
        {
            return new TokenModel
            {
                Token = AccessToken,
                RefreshToken = RefreshToken
            };
        }

        private static ClaimsPrincipal CreatePrincipal(string email)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) }));
        }

        #endregion
    }
}
