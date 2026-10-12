using StocksApp.Domain.Entities;

namespace StocksApp.Domain.RepositoryContracts
{
    /// <summary>
    /// An abstraction of the repository pattern for managing CandleMatch5s entities, providing methods for inserting or updating candle match data in the data store.
    /// </summary>
    public interface ICandleMatchRepository
    {
        Task UpsertAsync(CandleMatch5s candle, CancellationToken ct);
    }
}
