namespace StocksApp.Domain.RepositoryContracts
{
    /// <summary>
    /// An abstraction of the Unit of Work pattern, which is responsible for managing transactions and coordinating the work of multiple repositories.
    /// </summary>
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken);

        Task CommitTransactionAsync(CancellationToken cancellationToken);

        Task RollbackTransactionAsync(CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
