using Moq;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class UserServiceTest
    {
        private readonly IUserRepository _userRepository;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly IUserService _userService;
        public UserServiceTest()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userRepository = _userRepositoryMock.Object;
            _userService = new UserService(_userRepository);
        }

    }
}
