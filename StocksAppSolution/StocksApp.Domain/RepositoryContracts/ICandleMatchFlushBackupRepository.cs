using StocksApp.Domain.Entities;

namespace StocksApp.Domain.RepositoryContracts
{
    /// <summary>
    /// An abstraction of the repository pattern for managing CandleMatchFlushBackup entities, which are used to store pending flush operations for candle match data.
    /// </summary>
    public interface ICandleMatchFlushBackupRepository
    {
        Task AddAsync(CandleMatchFlushBackup backup, CancellationToken ct);
        Task<List<CandleMatchFlushBackup>> GetPendingAsync(int batchSize, CancellationToken ct);
        Task RemoveAsync(Guid id, CancellationToken ct);
        Task<int> CountPendingAsync(CancellationToken ct);
    }
}
