namespace StocksApp.OrdersWorker.ServiceContracts
{
    /// <summary>
    /// It reads the channel and routes each message to its handler by type.
    /// </summary>
    public interface IWorkerMessageDispatcher
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
