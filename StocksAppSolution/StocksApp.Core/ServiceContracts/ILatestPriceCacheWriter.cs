using StocksApp.Core.DTO.StockDTO;

namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// Tracks the most recent trade price per symbol, independent of any bar/bucket logic.
    /// </summary>
    public interface ILatestPriceCacheWriter
    {
        Task WriteAsync(PriceUpdateMessage tick, CancellationToken ct);
        Task DeleteAsync(string symbol, CancellationToken ct);
    }
}