using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.Domain.RepositoryContracts
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetUserByUserId(Guid userId);
        Task<ApplicationUser?> GetUserByEmail(string email);
        Task<ApplicationUser> AddUser(ApplicationUser user);
        Task<bool> UpdateUser(ApplicationUser user);
    }
}
