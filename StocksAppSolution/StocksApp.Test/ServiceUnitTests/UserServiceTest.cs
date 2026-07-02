using FluentAssertions;
using Moq;
using Xunit;
using StocksApp.Domain.Entities;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.Exceptions;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using StocksApp.Tests.Common.Builders;

namespace StocksApp.Test.ServiceUnitTests
{
    public class UserServiceTest
    {
        private const string HashedPassword = "dummy_instant_hash";
        private const string RefreshToken = "sample_refresh_token";
        private static readonly DateTime RefreshTokenExpiry = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly IUserService _userService;
        private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;

        public UserServiceTest()
        {
            _userRepositoryMock = new Mock<IGenericRepository<User>>();
            _passwordHasherMock = new Mock<IPasswordHasher>();

            _passwordHasherMock
                .Setup(ph => ph.HashPassword(It.IsAny<string>()))
                .Returns(HashedPassword);

            _userService = new UserService(_userRepositoryMock.Object, _passwordHasherMock.Object);
        }

        #region AddUser Tests

        [Fact]
        public async Task AddUser_EmailAlreadyExists_ThrowsClientException()
        {
            // Arrange
            var request = new UserAddRequestBuilder().Build();
            var existingUser = new UserBuilder().WithEmail(request.Email!).Build();

            _userRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<ISpecification<User>>()))
                .ReturnsAsync(existingUser); // Simulate existing user

            // Act
            Func<Task> action = async () => await _userService.AddUser(request);

            // Assert
            await action.Should().ThrowAsync<ClientException>();

            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
            _passwordHasherMock.Verify(ph => ph.HashPassword(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddUser_ValidUserRequest_ReturnsUserResponse()
        {
            // Arrange
            var request = new UserAddRequestBuilder().Build();

            _userRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<ISpecification<User>>()))
                .ReturnsAsync((User?)null); // Email is free

            _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User user) => user); // Return the passed entity

            // Act
            var response = await _userService.AddUser(request);

            // Assert
            response.Should().NotBeNull();
            response.UserName.Should().Be(request.UserName);
            response.Email.Should().Be(request.Email);
            response.CashBalance.Should().Be(100000);
            response.UserId.Should().NotBe(Guid.Empty);

            _passwordHasherMock.Verify(ph => ph.HashPassword(request.Password!), Times.Once);
            _userRepositoryMock.Verify(repo => repo.AddAsync(It.Is<User>(u =>
                u.UserName == request.UserName &&
                u.Email == request.Email &&
                u.PasswordHash == HashedPassword)), Times.Once);
        }

        #endregion

        #region GetUser Tests

        [Fact]
        public async Task GetUserById_ValidId_ReturnsUserResponse()
        {
            // Arrange
            var user = new UserBuilder().Build();

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(user.UserId))
                .ReturnsAsync(user);

            // Act
            var response = await _userService.GetUserById(user.UserId);

            // Assert
            response.Should().NotBeNull();
            response!.UserId.Should().Be(user.UserId);
            response.UserName.Should().Be(user.UserName);
            response.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetUserById_InvalidId_ReturnsNull()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User?)null);

            // Act
            var response = await _userService.GetUserById(Guid.NewGuid());

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByEmail_ValidEmail_ReturnsUserResponse()
        {
            // Arrange
            var user = new UserBuilder().Build();

            _userRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<ISpecification<User>>()))
                .ReturnsAsync(user);

            // Act
            var response = await _userService.GetUserByEmail(user.Email!);

            // Assert
            response.Should().NotBeNull();
            response!.UserId.Should().Be(user.UserId);
            response.UserName.Should().Be(user.UserName);
            response.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetUserByEmail_InvalidEmail_ReturnsNull()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<ISpecification<User>>()))
                .ReturnsAsync((User?)null);

            // Act
            var response = await _userService.GetUserByEmail("missing@test.com");

            // Assert
            response.Should().BeNull();
        }

        #endregion

        #region UpdateUser Tests

        [Fact]
        public async Task UpdateUser_UserNotFound_ThrowsClientException()
        {
            // Arrange
            var request = new UserUpdateRequest { UserId = Guid.NewGuid(), UserName = "UpdatedName", Email = "updated@test.com" };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(request.UserId))
                .ReturnsAsync((User?)null); // Not found

            // Act
            Func<Task> action = async () => await _userService.UpdateUser(request);

            // Assert
            await action.Should().ThrowAsync<ClientException>();

            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUser_EmailAlreadyExistsForAnotherUser_ThrowsClientException()
        {
            // Arrange
            var existingUser = new UserBuilder().Build();
            var request = new UserUpdateRequest { UserId = existingUser.UserId, UserName = "UpdatedName", Email = "updated@test.com" };
            var userWithSameEmail = new UserBuilder().WithEmail(request.Email!).Build(); // different UserId by default

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(request.UserId))
                .ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<ISpecification<User>>()))
                .ReturnsAsync(userWithSameEmail);

            // Act
            Func<Task> action = async () => await _userService.UpdateUser(request);

            // Assert
            await action.Should().ThrowAsync<ClientException>();

            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUser_ValidRequest_UpdatesAndReturnsResponse()
        {
            // Arrange
            var existingUser = new UserBuilder().Build();
            var request = new UserUpdateRequest { UserId = existingUser.UserId, UserName = "UpdatedName", Email = "updated@test.com" };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(request.UserId))
                .ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<ISpecification<User>>()))
                .ReturnsAsync((User?)null);

            _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _userService.UpdateUser(request);

            // Assert
            response.Should().NotBeNull();
            response.UserName.Should().Be(request.UserName);
            response.Email.Should().Be(request.Email);

            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<User>(u =>
                u.UserId == request.UserId &&
                u.UserName == request.UserName &&
                u.Email == request.Email)), Times.Once);
        }

        #endregion

        #region DeleteUser Tests

        [Fact]
        public async Task DeleteUser_UserNotFound_ThrowsClientException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            Func<Task> action = async () => await _userService.DeleteUser(userId);

            // Assert
            await action.Should().ThrowAsync<ClientException>();

            _userRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUser_UserExists_ReturnsTrue()
        {
            // Arrange
            var existingUser = new UserBuilder().Build();

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(existingUser.UserId))
                .ReturnsAsync(existingUser);

            _userRepositoryMock.Setup(repo => repo.DeleteAsync(existingUser))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.DeleteUser(existingUser.UserId);

            // Assert
            result.Should().BeTrue();
            _userRepositoryMock.Verify(repo => repo.DeleteAsync(It.Is<User>(u => u.UserId == existingUser.UserId)), Times.Once);
        }

        #endregion

        #region UpdateUserRefreshToken Tests

        [Fact]
        public async Task UpdateUserRefreshToken_UserNotFound_ThrowsClientException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User?)null); // Not found

            // Act
            Func<Task> action = async () => await _userService.UpdateUserRefreshToken(userId, RefreshToken, RefreshTokenExpiry);

            // Assert
            await action.Should().ThrowAsync<ClientException>();

            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserRefreshToken_ValidDetails_UpdatesAndReturnsResponse()
        {
            // Arrange
            var existingUser = new UserBuilder().Build();

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(existingUser.UserId))
                .ReturnsAsync(existingUser); // Found

            _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _userService.UpdateUserRefreshToken(existingUser.UserId, RefreshToken, RefreshTokenExpiry);

            // Assert
            response.Should().NotBeNull();
            response.UserId.Should().Be(existingUser.UserId);
            response.RefreshToken.Should().Be(RefreshToken);
            response.RefreshTokenExpiry.Should().Be(RefreshTokenExpiry);

            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<User>(u =>
                u.UserId == existingUser.UserId &&
                u.RefreshToken == RefreshToken &&
                u.RefreshTokenExpiry == RefreshTokenExpiry
            )), Times.Once);
        }

        #endregion
    }
}