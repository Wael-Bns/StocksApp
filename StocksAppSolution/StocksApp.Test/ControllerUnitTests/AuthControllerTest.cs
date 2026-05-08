using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Moq;
using StocksApp.Core.DTO.AuthenticationDTO;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.WebApi.Controllers;
using Xunit;

namespace StocksApp.Test.ControllerUnitTests
{
    public class AuthControllerTest
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _authController;

        public AuthControllerTest()
        {
            _authServiceMock = new Mock<IAuthService>();
            _authController = new AuthController(_authServiceMock.Object);
        }

        #region Register Tests

        [Fact]
        public async Task Register_ValidRequest_ReturnsOkWithResponse()
        {
            // Arrange
            var registerRequest = new UserAddRequest { UserName = "TestUser", Email = "test@test.com", Password = "Password123" };
            var authResponse = new AuthenticationResponse { Token = "jwt", RefreshToken = "refresh" };

            _authServiceMock.Setup(x => x.RegisterAsync(registerRequest))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _authController.Register(registerRequest);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(authResponse);
        }

        [Fact]
        public async Task Register_ArgumentException_ReturnsConflict()
        {
            // Arrange
            var registerRequest = new UserAddRequest { UserName = "TestUser", Email = "test@test.com", Password = "Password123" };

            _authServiceMock.Setup(x => x.RegisterAsync(registerRequest))
                .ThrowsAsync(new ArgumentException("A user with this email already exists."));

            // Act
            var result = await _authController.Register(registerRequest);

            // Assert
            var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflictResult.Value.Should().Be("A user with this email already exists.");
        }

        [Fact]
        public async Task Register_GenericException_ReturnsProblem()
        {
            // Arrange
            var registerRequest = new UserAddRequest { UserName = "TestUser", Email = "test@test.com", Password = "Password123" };

            _authServiceMock.Setup(x => x.RegisterAsync(registerRequest))
                .ThrowsAsync(new Exception("Registration failed"));

            // Act
            var result = await _authController.Register(registerRequest);

            // Assert
            var problemResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            problemResult.StatusCode.Should().Be(500); 
        }

        #endregion

        #region Login Tests

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithResponse()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "test@test.com", Password = "Password123" };
            var authResponse = new AuthenticationResponse { Token = "jwt", RefreshToken = "refresh" };

            _authServiceMock.Setup(x => x.LoginAsync(loginRequest))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _authController.Login(loginRequest);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(authResponse);
        }

        [Fact]
        public async Task Login_ArgumentException_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "invalid@test.com", Password = "Password123" };

            _authServiceMock.Setup(x => x.LoginAsync(loginRequest))
                .ThrowsAsync(new ArgumentException("Invalid email"));

            // Act
            var result = await _authController.Login(loginRequest);

            // Assert
            var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.Value.Should().Be("Invalid email");
        }

        [Fact]
        public async Task Login_GenericException_ReturnsProblem()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "valid@test.com", Password = "Password123" };

            _authServiceMock.Setup(x => x.LoginAsync(loginRequest))
                .ThrowsAsync(new Exception("Database exploded"));

            // Act
            var result = await _authController.Login(loginRequest);

            // Assert
            var problemResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            problemResult.StatusCode.Should().Be(500);
        }

        #endregion

        #region GenerateNewAccessToken Tests

        [Fact]
        public async Task GenerateNewAccessToken_ValidTokenModel_ReturnsOkWithResponse()
        {
            // Arrange
            var tokenModel = new TokenModel { Token = "old_jwt", RefreshToken = "old_refresh" };
            var authResponse = new AuthenticationResponse { Token = "new_jwt", RefreshToken = "new_refresh" };

            _authServiceMock.Setup(x => x.GenerateNewAccessTokenAsync(tokenModel))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _authController.GenerateNewAccessToken(tokenModel);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(authResponse);
        }

        [Fact]
        public async Task GenerateNewAccessToken_SecurityTokenException_ReturnsUnauthorized()
        {
            // Arrange
            var tokenModel = new TokenModel { Token = "invalid", RefreshToken = "invalid" };

            _authServiceMock.Setup(x => x.GenerateNewAccessTokenAsync(tokenModel))
                .ThrowsAsync(new SecurityTokenException("Invalid token"));

            // Act
            var result = await _authController.GenerateNewAccessToken(tokenModel);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.Value.Should().Be("Invalid token");
        }

        [Fact]
        public async Task GenerateNewAccessToken_ArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var tokenModel = new TokenModel { Token = "invalid", RefreshToken = "invalid" };

            _authServiceMock.Setup(x => x.GenerateNewAccessTokenAsync(tokenModel))
                .ThrowsAsync(new ArgumentException("Invalid client request"));

            // Act
            var result = await _authController.GenerateNewAccessToken(tokenModel);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Invalid client request");
        }
        
        [Fact]
        public async Task GenerateNewAccessToken_GenericException_ReturnsProblem()
        {
            // Arrange
            var tokenModel = new TokenModel { Token = "valid", RefreshToken = "valid" };

            _authServiceMock.Setup(x => x.GenerateNewAccessTokenAsync(tokenModel))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _authController.GenerateNewAccessToken(tokenModel);

            // Assert
            var problemResult = result.Should().BeOfType<ObjectResult>().Subject;
            problemResult.StatusCode.Should().Be(500);
        }

        #endregion
    }
}