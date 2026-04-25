using System.Security.Cryptography;
using System.Text;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IUserRepository _userRepository;
        public ApplicationUserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<ApplicationUserResponse> RegisterUser(ApplicationUserRegisterDTO registerDTO)
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetUserByEmail(registerDTO.Email!);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            // Simple Password hashing (Please use BCrypt or ASP.NET Identity PasswordHasher in production)
            using var hmac = new HMACSHA512();
            var passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password!)));

            var newUser = new ApplicationUser
            {
                UserId = Guid.NewGuid(),
                UserName = registerDTO.UserName,
                Email = registerDTO.Email,
                PasswordHash = passwordHash,
                CashBalance = 100000
            };

            var createdUser = await _userRepository.AddUser(newUser);
            return createdUser.ToApplicationUserResponse();
        }
    }
}
