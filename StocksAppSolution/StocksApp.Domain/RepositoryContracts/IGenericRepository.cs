// StocksApp.Domain/RepositoryContracts/IGenericRepository.cs
using StocksApp.Domain.Specifications;

namespace StocksApp.Domain.RepositoryContracts
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<T?> GetAsync(ISpecification<T> spec);
        Task<List<T>> ListAsync(ISpecification<T> spec);
        Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec);
        Task<int> CountAsync(ISpecification<T> spec);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}