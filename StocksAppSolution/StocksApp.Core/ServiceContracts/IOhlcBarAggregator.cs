using StocksApp.Core.DTO.StockDTO;

namespace StocksApp.Core.ServiceContracts
{
    public interface IOhlcBarAggregator
    {
        Task ApplyTickAsync(PriceUpdateMessage tick, CancellationToken ct);
        Task FlushIdleBarsAsync(DateTimeOffset now, CancellationToken ct);
        Task HydrateFromCacheAsync(CancellationToken ct);

        /// <summary>D8: flushes the symbol's open bar (if any) and clears all its Redis keys.</summary>
        Task FlushAndRemoveSymbolAsync(string symbol, CancellationToken ct);

        /// <summary>Clears in-memory state on leadership loss.</summary>
        void Reset();
    }
}