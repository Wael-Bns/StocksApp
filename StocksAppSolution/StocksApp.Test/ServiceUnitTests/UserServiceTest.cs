using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;

namespace StocksApp.Test.ServiceUnitTests
{
    public class UserServiceTest
    {
        private readonly IUserService _userService;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public UserServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        #region AddUser Tests

        [Fact]
        public async Task AddUser_EmailAlreadyExists_ThrowsArgumentException()
        {
            // Arrange
            var request = new UserAddRequest { Email = "test@test.com", UserName = "TestUser", Password = "Password123" };
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email))
                .ReturnsAsync(new User { Email = request.Email }); // Simulate existing user

            // Act
            Func<Task> action = async () => await _userService.AddUser(request);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>()
                .WithMessage("A user with this email already exists.");
        }

        [Fact]
        public async Task AddUser_ValidUserRequest_ReturnsUserResponse()
        {
            // Arrange
            var request = new UserAddRequest { Email = "new@test.com", UserName = "NewUser", Password = "password123" };
            
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email))
                .ReturnsAsync((User?)null); // Email is free

            _userRepositoryMock.Setup(repo => repo.AddUser(It.IsAny<User>()))
                .ReturnsAsync((User user) => user); // Return the passed entity

            // Act
            var response = await _userService.AddUser(request);

            // Assert
            response.Should().NotBeNull();
            response.UserName.Should().Be(request.UserName);
            response.Email.Should().Be(request.Email);
            response.CashBalance.Should().Be(100000);
            response.UserId.Should().NotBe(Guid.Empty);
            
            _userRepositoryMock.Verify(repo => repo.AddUser(It.IsAny<User>()), Times.Once);
        }

        #endregion

        #region GetUser Tests

        [Fact]
        public async Task GetUserById_ValidId_ReturnsUserResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId, UserName = "Test", Email = "test@test.com" };

            _userRepositoryMock.Setup(repo => repo.GetUserById(userId))
                .ReturnsAsync(user);

            // Act
            var response = await _userService.GetUserById(userId);

            // Assert
            response.Should().NotBeNull();
            response!.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task GetUserById_InvalidId_ReturnsNull()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetUserById(It.IsAny<Guid>()))
                .ReturnsAsync((User?)null);

            // Act
            var response = await _userService.GetUserById(Guid.NewGuid());

            // Assert
            response.Should().BeNull();
        }

        #endregion

        #region UpdateUser Tests

        [Fact]
        public async Task UpdateUser_UserNotFound_ThrowsArgumentException()
        {
            // Arrange
            var request = new UserUpdateRequest { UserId = Guid.NewGuid(), UserName = "Updated", Email = "updated@test.com" };

            _userRepositoryMock.Setup(repo => repo.GetUserById(request.UserId))
                .ReturnsAsync((User?)null); // Not found

            // Act
            Func<Task> action = async () => await _userService.UpdateUser(request);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>()
                .WithMessage("User not found.");
        }

        [Fact]
        public async Task UpdateUser_ValidRequest_UpdatesAndReturnsResponse()
        {
            // Arrange
            var request = new UserUpdateRequest { UserId = Guid.NewGuid(), UserName = "UpdatedName", Email = "updated@test.com" };
            var existingUser = new User { UserId = request.UserId, UserName = "OldName", Email = "old@test.com" };

            _userRepositoryMock.Setup(repo => repo.GetUserById(request.UserId))
                .ReturnsAsync(existingUser); // Found

            _userRepositoryMock.Setup(repo => repo.UpdateUser(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            var response = await _userService.UpdateUser(request);

            // Assert
            response.Should().NotBeNull();
            response.UserName.Should().Be("UpdatedName");
            response.Email.Should().Be("updated@test.com");
            
            _userRepositoryMock.Verify(repo => repo.UpdateUser(It.IsAny<User>()), Times.Once);
        }

        #endregion

        #region DeleteUser Tests

        [Fact]
        public async Task DeleteUser_UserNotFound_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.GetUserById(userId))
                .ReturnsAsync((User?)null);

            // Act
            Func<Task> action = async () => await _userService.DeleteUser(userId);

            // Assert
            await action.Should().ThrowAsync<ArgumentException>()
                .WithMessage("User not found.");
        }

        [Fact]
        public async Task DeleteUser_UserExists_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.GetUserById(userId))
                .ReturnsAsync(new User { UserId = userId });

            _userRepositoryMock.Setup(repo => repo.DeleteUser(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.DeleteUser(userId);

            // Assert
            result.Should().BeTrue();
            _userRepositoryMock.Verify(repo => repo.DeleteUser(userId), Times.Once);
        }

        #endregion
    }
}
