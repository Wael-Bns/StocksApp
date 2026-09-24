namespace StocksApp.Core.ServiceContracts
{
    public interface IStockSubscriptionTracker
    {
        Task AddInterestAsync(string connectionId, string symbol, CancellationToken ct = default);
        Task RemoveInterestAsync(string connectionId, string symbol, CancellationToken ct = default);
        Task<IReadOnlyCollection<string>> RemoveConnectionAsync(string connectionId, CancellationToken ct = default);
    }
}
