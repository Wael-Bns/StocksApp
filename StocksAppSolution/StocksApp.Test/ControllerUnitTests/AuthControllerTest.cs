using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
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
        public async Task Register_ServiceThrowsException_ReturnsProblem()
        {
            // Arrange
            var registerRequest = new UserAddRequest { UserName = "TestUser", Email = "test@test.com", Password = "Password123" };

            _authServiceMock.Setup(x => x.RegisterAsync(registerRequest))
                .ThrowsAsync(new Exception("Registration failed"));

            // Act
            var result = await _authController.Register(registerRequest);

            // Assert
            var problemResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            problemResult.StatusCode.Should().Be(500); // Problem() returns 500 status by default
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
        public async Task Login_ServiceThrowsException_ReturnsProblem()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "invalid@test.com", Password = "Password123" };

            _authServiceMock.Setup(x => x.LoginAsync(loginRequest))
                .ThrowsAsync(new ArgumentException("Invalid email"));

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
        public async Task GenerateNewAccessToken_ServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var tokenModel = new TokenModel { Token = "invalid", RefreshToken = "invalid" };

            _authServiceMock.Setup(x => x.GenerateNewAccessTokenAsync(tokenModel))
                .ThrowsAsync(new ArgumentException("Invalid refresh token"));

            // Act
            var result = await _authController.GenerateNewAccessToken(tokenModel);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Invalid refresh token");
        }

        #endregion
    }
}