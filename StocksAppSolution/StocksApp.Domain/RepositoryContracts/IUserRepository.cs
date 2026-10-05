using StocksApp.Domain.Entities;

namespace StocksApp.Domain.RepositoryContracts
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid userId, CancellationToken ct);
    }
}
