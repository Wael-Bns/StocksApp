using StocksApp.Core.DTO.CandleDTO;

namespace StocksApp.Core.ServiceContracts
{
    public interface ICandleCache
    {
        Task SetLatestPriceAsync(LatestPriceSnapshot snapshot, CancellationToken ct);

        Task SetActiveBarAsync(ActiveBarSnapshot bar, CancellationToken ct);
        Task<ActiveBarSnapshot?> GetActiveBarAsync(string symbol, CancellationToken ct);
        Task DeleteActiveBarAsync(string symbol, CancellationToken ct);
        IAsyncEnumerable<ActiveBarSnapshot> GetAllActiveBarsAsync(CancellationToken ct);
    }
}