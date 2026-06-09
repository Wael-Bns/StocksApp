using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Exceptions;

namespace StocksApp.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserResponse> AddUser(UserAddRequest request)
        {
            var existingUser = await _userRepository.GetUserByEmail(request.Email!);
            if (existingUser != null)
            {
                throw new EmailAlreadyExistsException();
            }

            var user = request.ToUser();

            user.PasswordHash = _passwordHasher.HashPassword(request.Password!); 

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

        public async Task<UserResponse> UpdateUser(UserUpdateRequest userUpdateRequest)
        {
            var existingUser = await _userRepository.GetUserById(userUpdateRequest.UserId);
            if (existingUser == null)
            {
                throw new UserNotFoundException();
            }

            var userWithSameEmail = await _userRepository.GetUserByEmail(userUpdateRequest.Email!);
            
            if(userWithSameEmail != null && userWithSameEmail.UserId != userUpdateRequest.UserId)
            {
                throw new EmailAlreadyExistsException();
            }

            // Update allowed properties
            existingUser.UserName = userUpdateRequest.UserName;
            existingUser.Email = userUpdateRequest.Email;

            var updatedUser = await _userRepository.UpdateUser(existingUser);

            return updatedUser.ToUserResponse();
        }

        public async Task<bool> DeleteUser(Guid userId)
        {
            var existingUser = await _userRepository.GetUserById(userId);
            if (existingUser == null)
            {
                throw new UserNotFoundException();
            }

            return await _userRepository.DeleteUser(userId);
        }
        public async Task<UserResponse> UpdateUserRefreshToken(Guid userId, string refreshToken, DateTime refreshTokenExpiry)
        {
            var existingUser = await _userRepository.GetUserById(userId);
            if (existingUser == null)
            {
                throw new UserNotFoundException();
            }

            existingUser.RefreshToken = refreshToken;
            existingUser.RefreshTokenExpiry = refreshTokenExpiry;

            var updatedUser = await _userRepository.UpdateUser(existingUser);

            return updatedUser.ToUserResponse();
        }
    }
}