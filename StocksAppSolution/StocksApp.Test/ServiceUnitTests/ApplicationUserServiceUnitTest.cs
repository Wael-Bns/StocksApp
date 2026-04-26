using FluentAssertions;
using Moq;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.Services;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class ApplicationUserServiceUnitTest
    {
        private readonly IUserRepository _userRepository;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly ApplicationUserService _applicationUserService;

        private ApplicationUserRegisterDTO _applicationUserRegisterDTO = new ApplicationUserRegisterDTO
        {
            UserName = "test",
            Email = "test@example.com",
            Password = "test123",
            ConfirmPassword = "test123"
        };
        private ApplicationUser _applicationUser = new ApplicationUser
        {
            UserId = new Guid("1FE93A57-986F-49AD-A66A-54160ACE21FD"),
            UserName = "test",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            CashBalance = 100000
        };
        public ApplicationUserServiceUnitTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userRepository = _userRepositoryMock.Object;
            _applicationUserService = new ApplicationUserService(_userRepository);
        }

        #region RegisterUser Tests
        [Fact]
        public async Task RegisterUser_ShouldRegisterSuccessfully()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(It.IsAny<string>()))
                .ReturnsAsync(null as ApplicationUser);
            _userRepositoryMock.Setup(repo => repo.AddUser(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(_applicationUser);
            // Act
            ApplicationUserResponse response = await _applicationUserService.RegisterUser(_applicationUserRegisterDTO);
            // Assert
            response.Should().Be(_applicationUser.ToApplicationUserResponse());
        }
        [Fact]
        public async Task RegisterUser_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(It.IsAny<string>()))
                .ReturnsAsync(_applicationUser);
            // Act
            Func<Task> act = async () => await _applicationUserService.RegisterUser(_applicationUserRegisterDTO);
            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("User with this email already exists.");
        }
        [Fact]
        public async Task RegisterUser_ShouldHashPassword()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(It.IsAny<string>()))
                .ReturnsAsync(null as ApplicationUser);
            _userRepositoryMock.Setup(repo => repo.AddUser(It.IsAny<ApplicationUser>()))
                .ReturnsAsync((ApplicationUser user) => user);
            // Act
            ApplicationUserResponse response = await _applicationUserService.RegisterUser(_applicationUserRegisterDTO);
            // Assert
            _userRepositoryMock.Verify(repo => repo.AddUser(It.Is<ApplicationUser>(user =>
                user.PasswordHash != null && user.PasswordHash != _applicationUserRegisterDTO.Password)), Times.Once);
        }
        [Fact]
        public async Task RegisterUser_ShouldSetInitialCashBalance()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(It.IsAny<string>()))
                .ReturnsAsync(null as ApplicationUser);
            _userRepositoryMock.Setup(repo => repo.AddUser(It.IsAny<ApplicationUser>()))
                .ReturnsAsync((ApplicationUser user) => user);
            // Act
            ApplicationUserResponse response = await _applicationUserService.RegisterUser(_applicationUserRegisterDTO);
            // Assert
            _userRepositoryMock.Verify(repo => repo.AddUser(It.Is<ApplicationUser>(user =>
                user.CashBalance == 100000)), Times.Once);
        }
        #endregion

    }
}
