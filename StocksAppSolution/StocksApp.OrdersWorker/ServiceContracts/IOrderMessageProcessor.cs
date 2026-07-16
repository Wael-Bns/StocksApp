namespace StocksApp.OrdersWorker.ServiceContracts
{
    /// <summary>
    /// Handles messages received from the message broker and processes them accordingly.
    /// </summary>
    public interface IOrderMessageProcessor
    {
        Task StartAsync(CancellationToken cancellationToken);
    }
}
