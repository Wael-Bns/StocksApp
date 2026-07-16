namespace StocksApp.OrdersWorker.ServiceContracts
{
    /// <summary>
    /// Responsible for initializing pending orders from the database into the in-memory store when the worker starts.
    /// </summary>
    public interface IPendingOrdersInitializer
    {
        Task StartAsync(CancellationToken cancellationToken);
    }
}
