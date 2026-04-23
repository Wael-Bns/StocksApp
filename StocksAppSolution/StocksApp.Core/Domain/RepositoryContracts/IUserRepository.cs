using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.Domain.RepositoryContracts
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetUserByUserId(Guid userId);
        Task<bool> UpdateUser(ApplicationUser user);
    }
}
