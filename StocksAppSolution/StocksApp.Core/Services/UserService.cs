using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserResponse> AddUser(UserAddRequest request)
        {
            var existingUser = await _userRepository.GetUserByEmail(request.Email!);
            if (existingUser != null)
            {
                throw new ArgumentException("A user with this email already exists.");
            }

            var user = request.ToUser();

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password); 

            var createdUser = await _userRepository.AddUser(user);

            return createdUser.ToUserResponse();
        }

        public async Task<UserResponse?> GetUserById(Guid userId)
        {
            var user = await _userRepository.GetUserById(userId);
            return user?.ToUserResponse();
        }

        public async Task<UserResponse?> GetUserByEmail(string email)
        {
            var user = await _userRepository.GetUserByEmail(email);
            return user?.ToUserResponse();
        }

        public async Task<UserResponse> UpdateUser(UserUpdateRequest request)
        {
            var existingUser = await _userRepository.GetUserById(request.UserId);
            if (existingUser == null)
            {
                throw new ArgumentException("User not found.");
            }

            // Update allowed properties
            existingUser.UserName = request.UserName;
            existingUser.Email = request.Email;

            var updatedUser = await _userRepository.UpdateUser(existingUser);

            return updatedUser.ToUserResponse();
        }

        public async Task<bool> DeleteUser(Guid userId)
        {
            var existingUser = await _userRepository.GetUserById(userId);
            if (existingUser == null)
            {
                throw new ArgumentException("User not found.");
            }

            return await _userRepository.DeleteUser(userId);
        }
    }
}