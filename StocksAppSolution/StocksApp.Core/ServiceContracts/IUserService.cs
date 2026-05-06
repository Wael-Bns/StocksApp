using StocksApp.Core.DTO.UsersDTO;

namespace StocksApp.Core.ServiceContracts
{
    public interface IUserService
    {
        Task<UserResponse> AddUser(UserAddRequest request);
        Task<UserResponse?> GetUserById(Guid userId);
        Task<UserResponse?> GetUserByEmail(string email);
        Task<UserResponse> UpdateUser(UserUpdateRequest request);
        Task<bool> DeleteUser(Guid userId);
    }
}