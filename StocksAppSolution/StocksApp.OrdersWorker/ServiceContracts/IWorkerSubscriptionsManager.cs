namespace StocksApp.OrdersWorker.ServiceContracts
{
    /// <summary>
    /// Responsible for managing the subscriptions to stock price updates based on the orders in the database.
    /// </summary>
    public interface IWorkerSubscriptionsManager
    {
        Task RefreshSubscriptionsPeriodically(CancellationToken cancellationToken);
    }
}
