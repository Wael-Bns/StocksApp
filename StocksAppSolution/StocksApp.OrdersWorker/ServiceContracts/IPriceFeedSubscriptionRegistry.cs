namespace StocksApp.OrdersWorker.ServiceContracts
{
    /// <summary>
    ///  Maintains which symbols the worker is subscribed to on the price feed.
    /// </summary>
    public interface IPriceFeedSubscriptionRegistry
    {
        Task EnsureSubscribedAsync(string stockSymbol, CancellationToken cancellationToken);
        Task UnsubscribeAsync(string stockSymbol, CancellationToken cancellationToken);
    }
}
