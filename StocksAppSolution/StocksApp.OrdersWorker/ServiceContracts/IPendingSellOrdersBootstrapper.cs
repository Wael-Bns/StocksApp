namespace StocksApp.OrdersWorker.ServiceContracts
{
    /// <summary>
    /// Prepares the worker's runtime state from persisted data at startup.
    /// </summary>
    public interface IPendingSellOrdersBootstrapper
    {
        Task RestoreAsync(CancellationToken cancellationToken);
    }
}
