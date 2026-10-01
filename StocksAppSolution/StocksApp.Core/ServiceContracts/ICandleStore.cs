using StocksApp.Core.Candles;

namespace StocksApp.Core.ServiceContracts
{
    public interface ICandleStore
    {
        Task UpsertClosedCandleAsync(OhlcBar bar, CancellationToken ct);
    }
}