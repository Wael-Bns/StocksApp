using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.Exceptions;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;

namespace StocksApp.Test.ServiceUnitTests
{
    public class UserServiceTest
    {
        private const string TestEmail = "test@test.com";
        private const string UpdatedEmail = "updated@test.com";
        private const string TestUserName = "TestUser";
        private const string UpdatedUserName = "UpdatedName";
        private const string TestPassword = "Password123";
        private const string HashedPassword = "dummy_instant_hash";
        private const string RefreshToken = "sample_refresh_token";
        private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid OtherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly DateTime RefreshTokenExpiry = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly IUserService _userService;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock; 

        public UserServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
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
            var request = new UserAddRequest { Email = TestEmail, UserName = TestUserName, Password = TestPassword };
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email))
                .ReturnsAsync(new User { UserId = TestUserId, Email = request.Email, UserName = TestUserName }); // Simulate existing user

            // Act
            Func<Task> action = async () => await _userService.AddUser(request);

            // Assert
            await action.Should().ThrowAsync<ClientException>();
                
            _userRepositoryMock.Verify(repo => repo.AddUser(It.IsAny<User>()), Times.Never);
            _passwordHasherMock.Verify(ph => ph.HashPassword(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddUser_ValidUserRequest_ReturnsUserResponse()
        {
            // Arrange
            var request = new UserAddRequest { Email = TestEmail, UserName = TestUserName, Password = TestPassword };
            
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
            
            _passwordHasherMock.Verify(ph => ph.HashPassword(TestPassword), Times.Once);
            _userRepositoryMock.Verify(repo => repo.AddUser(It.Is<User>(u =>
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
            var user = new User { UserId = TestUserId, UserName = TestUserName, Email = TestEmail };

            _userRepositoryMock.Setup(repo => repo.GetUserById(TestUserId))
                .ReturnsAsync(user);

            // Act
            var response = await _userService.GetUserById(TestUserId);

            // Assert
            response.Should().NotBeNull();
            response!.UserId.Should().Be(TestUserId);
            response.UserName.Should().Be(TestUserName);
            response.Email.Should().Be(TestEmail);
        }

        [Fact]
        public async Task GetUserById_InvalidId_ReturnsNull()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetUserById(It.IsAny<Guid>()))
                .ReturnsAsync((User?)null);

            // Act
            var response = await _userService.GetUserById(TestUserId);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByEmail_ValidEmail_ReturnsUserResponse()
        {
            // Arrange
            var user = new User { UserId = TestUserId, UserName = TestUserName, Email = TestEmail };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(TestEmail))
                .ReturnsAsync(user);

            // Act
            var response = await _userService.GetUserByEmail(TestEmail);

            // Assert
            response.Should().NotBeNull();
            response!.UserId.Should().Be(TestUserId);
            response.UserName.Should().Be(TestUserName);
            response.Email.Should().Be(TestEmail);
        }

        [Fact]
        public async Task GetUserByEmail_InvalidEmail_ReturnsNull()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(TestEmail))
                .ReturnsAsync((User?)null);

            // Act
            var response = await _userService.GetUserByEmail(TestEmail);

            // Assert
            response.Should().BeNull();
        }

        #endregion

        #region UpdateUser Tests

        [Fact]
        public async Task UpdateUser_UserNotFound_ThrowsClientException()
        {
            // Arrange
            var request = new UserUpdateRequest { UserId = TestUserId, UserName = UpdatedUserName, Email = UpdatedEmail };

            _userRepositoryMock.Setup(repo => repo.GetUserById(request.UserId))
                .ReturnsAsync((User?)null); // Not found

            // Act
            Func<Task> action = async () => await _userService.UpdateUser(request);

            // Assert
            await action.Should().ThrowAsync<ClientException>();
                
            _userRepositoryMock.Verify(repo => repo.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUser_EmailAlreadyExistsForAnotherUser_ThrowsClientException()
        {
            // Arrange
            var request = new UserUpdateRequest { UserId = TestUserId, UserName = UpdatedUserName, Email = UpdatedEmail };
            var existingUser = new User { UserId = TestUserId, UserName = TestUserName, Email = TestEmail };
            var userWithSameEmail = new User { UserId = OtherUserId, UserName = "OtherUser", Email = UpdatedEmail };

            _userRepositoryMock.Setup(repo => repo.GetUserById(request.UserId))
                .ReturnsAsync(existingUser);
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email!))
                .ReturnsAsync(userWithSameEmail);

            // Act
            Func<Task> action = async () => await _userService.UpdateUser(request);

            // Assert
            await action.Should().ThrowAsync<ClientException>();

            _userRepositoryMock.Verify(repo => repo.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUser_ValidRequest_UpdatesAndReturnsResponse()
        {
            // Arrange
            var request = new UserUpdateRequest { UserId = TestUserId, UserName = UpdatedUserName, Email = UpdatedEmail };
            var existingUser = new User { UserId = request.UserId, UserName = TestUserName, Email = TestEmail };

            _userRepositoryMock.Setup(repo => repo.GetUserById(request.UserId))
                .ReturnsAsync(existingUser); // Found
            _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email!))
                .ReturnsAsync((User?)null);

            _userRepositoryMock.Setup(repo => repo.UpdateUser(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            var response = await _userService.UpdateUser(request);

            // Assert
            response.Should().NotBeNull();
            response.UserName.Should().Be(request.UserName);
            response.Email.Should().Be(request.Email);
            
            _userRepositoryMock.Verify(repo => repo.UpdateUser(It.Is<User>(u => 
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
            _userRepositoryMock.Setup(repo => repo.GetUserById(TestUserId))
                .ReturnsAsync((User?)null);

            // Act
            Func<Task> action = async () => await _userService.DeleteUser(TestUserId);

            // Assert
            await action.Should().ThrowAsync<ClientException>();
                
            _userRepositoryMock.Verify(repo => repo.DeleteUser(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUser_UserExists_ReturnsTrue()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetUserById(TestUserId))
                .ReturnsAsync(new User { UserId = TestUserId });

            _userRepositoryMock.Setup(repo => repo.DeleteUser(TestUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.DeleteUser(TestUserId);

            // Assert
            result.Should().BeTrue();
            _userRepositoryMock.Verify(repo => repo.DeleteUser(TestUserId), Times.Once);
        }

        #endregion

        #region UpdateUserRefreshToken Tests

        [Fact]
        public async Task UpdateUserRefreshToken_UserNotFound_ThrowsClientException()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetUserById(TestUserId))
                .ReturnsAsync((User?)null); // Not found

            // Act
            Func<Task> action = async () => await _userService.UpdateUserRefreshToken(TestUserId, RefreshToken, RefreshTokenExpiry);

            // Assert
            await action.Should().ThrowAsync<ClientException>();

            _userRepositoryMock.Verify(repo => repo.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserRefreshToken_ValidDetails_UpdatesAndReturnsResponse()
        {
            // Arrange
            var existingUser = new User { UserId = TestUserId, UserName = TestUserName, Email = TestEmail };

            _userRepositoryMock.Setup(repo => repo.GetUserById(TestUserId))
                .ReturnsAsync(existingUser); // Found

            _userRepositoryMock.Setup(repo => repo.UpdateUser(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            // Act
            var response = await _userService.UpdateUserRefreshToken(TestUserId, RefreshToken, RefreshTokenExpiry);

            // Assert
            response.Should().NotBeNull();
            response.UserId.Should().Be(TestUserId);
            response.RefreshToken.Should().Be(RefreshToken);
            response.RefreshTokenExpiry.Should().Be(RefreshTokenExpiry);

            // Verify the properties were accurately updated on the user that was sent to the repository
            _userRepositoryMock.Verify(repo => repo.UpdateUser(It.Is<User>(u => 
                u.UserId == TestUserId &&
                u.RefreshToken == RefreshToken &&
                u.RefreshTokenExpiry == RefreshTokenExpiry
            )), Times.Once);
        }

        #endregion
    }
}
