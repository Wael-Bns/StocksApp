using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Exceptions;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Specifications;

namespace StocksApp.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IGenericRepository<User> userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserResponse> AddUser(UserAddRequest request)
        {
            var userByEmailSpec = new UserByEmailSpecification(request.Email!);
            var existingUser = await _userRepository.GetAsync(userByEmailSpec);
            if (existingUser != null)
            {
                throw new EmailAlreadyExistsException();
            }

            var user = request.ToUser();
            user.PasswordHash = _passwordHasher.HashPassword(request.Password!);

            var createdUser = await _userRepository.AddAsync(user);
            return createdUser.ToUserResponse();
        }

        public async Task<UserResponse?> GetUserById(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.ToUserResponse();
        }

        public async Task<UserResponse?> GetUserByEmail(string email)
        {
            var spec = new UserByEmailSpecification(email);
            var user = await _userRepository.GetAsync(spec);
            return user?.ToUserResponse();
        }

        public async Task<UserResponse> UpdateUser(UserUpdateRequest userUpdateRequest)
        {
            var existingUser = await _userRepository.GetByIdAsync(userUpdateRequest.UserId);
            if (existingUser == null)
            {
                throw new UserNotFoundException();
            }

            var emailSpec = new UserByEmailSpecification(userUpdateRequest.Email!);
            var userWithSameEmail = await _userRepository.GetAsync(emailSpec);

            if (userWithSameEmail != null && userWithSameEmail.UserId != userUpdateRequest.UserId)
            {
                throw new EmailAlreadyExistsException();
            }

            // Update allowed properties
            existingUser.UserName = userUpdateRequest.UserName;
            existingUser.Email = userUpdateRequest.Email;

            await _userRepository.UpdateAsync(existingUser);
            return existingUser.ToUserResponse();
        }

        public async Task<bool> DeleteUser(Guid userId)
        {
            var existingUser = await _userRepository.GetByIdAsync(userId);
            if (existingUser == null)
            {
                throw new UserNotFoundException();
            }

            await _userRepository.DeleteAsync(existingUser);
            return true;
        }

        public async Task<UserResponse> UpdateUserRefreshToken(Guid userId, string refreshToken, DateTime refreshTokenExpiry)
        {
            var existingUser = await _userRepository.GetByIdAsync(userId);
            if (existingUser == null)
            {
                throw new UserNotFoundException();
            }

            existingUser.RefreshToken = refreshToken;
            existingUser.RefreshTokenExpiry = refreshTokenExpiry;

            await _userRepository.UpdateAsync(existingUser);
            return existingUser.ToUserResponse();
        }
    }
}