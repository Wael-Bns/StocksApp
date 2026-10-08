using StocksApp.Core.DTO.CandleDTO;
using StocksApp.Core.ServiceContracts;

public sealed class NullCandleCache : ICandleCache
{
    public Task SetActiveBarAsync(ActiveBarSnapshot bar, CancellationToken ct) => Task.CompletedTask;
    public Task<ActiveBarSnapshot?> GetActiveBarAsync(string symbol, CancellationToken ct) =>
        Task.FromResult<ActiveBarSnapshot?>(null);
    public Task DeleteActiveBarAsync(string symbol, CancellationToken ct) => Task.CompletedTask;
    public Task SetLatestPriceAsync(LatestPriceSnapshot snapshot, CancellationToken ct) => Task.CompletedTask;
    public Task<LatestPriceSnapshot?> GetLatestPriceAsync(string symbol, CancellationToken ct) =>
        Task.FromResult<LatestPriceSnapshot?>(null);
    public Task DeleteLatestPriceAsync(string symbol, CancellationToken ct) => Task.CompletedTask;

    public async IAsyncEnumerable<ActiveBarSnapshot> GetAllActiveBarsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        await Task.CompletedTask;
        yield break;
    }
}