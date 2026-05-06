using StocksApp.Core.DTO.UsersDTO;

namespace StocksApp.Core.ServiceContracts
{
    public interface IUserService
    {
        Task<UserResponse> AddUser(UserAddRequest userAddRequest);
        Task<UserResponse?> GetUserById(Guid userId);
        Task<UserResponse?> GetUserByEmail(string email);
        Task<UserResponse> UpdateUser(UserUpdateRequest userUpdateRequest);
        Task<bool> DeleteUser(Guid userId);
        public Task<UserResponse> UpdateUserRefreshToken(Guid userId, string refreshToken, DateTime refreshTokenExpiry);

    }
}