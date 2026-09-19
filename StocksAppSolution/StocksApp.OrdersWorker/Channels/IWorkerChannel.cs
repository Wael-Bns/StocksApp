using StocksApp.OrdersWorker.Messages;

namespace StocksApp.OrdersWorker.Channels
{
    /// <summary>
    /// Carries messages from producers to the consumer inside the worker.
    /// </summary>
    public interface IWorkerChannel
    {
        ValueTask EnqueueAsync(WorkerMessage message, CancellationToken cancellationToken = default);
        IAsyncEnumerable<WorkerMessage> ReadAllAsync(CancellationToken cancellationToken);
    }
}
