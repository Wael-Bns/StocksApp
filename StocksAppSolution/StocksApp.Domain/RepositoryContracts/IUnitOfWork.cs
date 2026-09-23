namespace StocksApp.Domain.RepositoryContracts
{
    /// <summary>
    /// An abstraction of the Unit of Work pattern, which is responsible for managing transactions and coordinating the work of multiple repositories.
    /// </summary>
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
