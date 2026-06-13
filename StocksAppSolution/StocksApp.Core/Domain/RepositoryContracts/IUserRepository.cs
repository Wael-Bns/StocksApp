using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.Domain.RepositoryContracts
{
    public interface IUserRepository
    {
        Task<User> AddUser(User user);
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserById(Guid userId);
        Task<User> UpdateUser(User user);
        Task<bool> DeleteUser(Guid userId);
    }
}
