using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.WebApi.Controllers;
using Xunit;

namespace StocksApp.Test.ControllersUnitTests
{
    public class AuthControllerUnitTest
    {
        private readonly Mock<IApplicationUserService> _applicationUserServiceMock;
        private readonly IApplicationUserService _applicationUserService;
        private ApplicationUserRegisterDTO registerDTO = new ApplicationUserRegisterDTO
        {
            UserName = "test",
            Email = "test@example.com",
            Password = "test123",
            ConfirmPassword = "test123"
        };
        private ApplicationUserResponse applicationUserResponse = new ApplicationUserResponse
        {
            UserId = new Guid("1FE93A57-986F-49AD-A66A-54160ACE21FD"),
            UserName = "test",
            Email = "test@example.com",
            CashBalance = 100000
        };
        public AuthControllerUnitTest()
        {
            _applicationUserServiceMock = new Mock<IApplicationUserService>();
            _applicationUserService = _applicationUserServiceMock.Object;
        }
        #region Register Tests
        [Fact]
        public async Task Register_ShouldReturnCreatedAtActionResult_WhenRegistrationIsSuccessful()
        {
            // Arrange
            _applicationUserServiceMock.Setup(service => service.RegisterUser(It.IsAny<ApplicationUserRegisterDTO>()))
                .ReturnsAsync(applicationUserResponse);

            AuthController authController = new AuthController(_applicationUserService);

            // Act
            var actual = await authController.Register(registerDTO);

            // Assert
            // Because Register returns ActionResult<ApplicationUserResponse>, 
            // the CreatedAtActionResult is stored inside actual.Result
            var createdAtActionResult = actual.Result.Should().BeOfType<CreatedAtActionResult>().Subject;

            createdAtActionResult.ActionName.Should().Be(nameof(AuthController.Register));
            createdAtActionResult.Value.Should().BeEquivalentTo(applicationUserResponse);
        }
        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
        {
            // Arrange
            _applicationUserServiceMock.Setup(service => service.RegisterUser(It.IsAny<ApplicationUserRegisterDTO>()))
                .ThrowsAsync(new InvalidOperationException("User with this email already exists."));

            AuthController authController = new AuthController(_applicationUserService);

            // Act
            var actual = await authController.Register(registerDTO);

            // Assert
            var badRequestResult = actual.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("User with this email already exists.");
        }
        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            AuthController authController = new AuthController(_applicationUserService);
            authController.ModelState.AddModelError("Email", "Email is required.");
            // Act
            var actual = await authController.Register(registerDTO);
            // Assert
            var badRequestResult = actual.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var modelState = badRequestResult.Value.Should().BeOfType<SerializableError>().Subject;
            modelState.ContainsKey("Email").Should().BeTrue();
        }
        #endregion
    }
}
