namespace StocksApp.Core.ServiceContracts
{
    public interface ITrackedSymbolStore
    {
        Task<IReadOnlySet<string>> GetActiveSymbolsAsync(CancellationToken ct);
    }
}