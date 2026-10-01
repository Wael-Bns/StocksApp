using StocksApp.Domain.Entities;

namespace StocksApp.Domain.RepositoryContracts
{
    public interface ICandleRepository
    {
        Task UpsertAsync(Candle1m candle, CancellationToken ct);
    }
}