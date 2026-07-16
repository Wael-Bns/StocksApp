using StocksApp.OrdersWorker.Messages;

namespace StocksApp.OrdersWorker.Channels
{
    public interface IWorkerChannel
    {
        ValueTask EnqueueAsync(WorkerMessage message, CancellationToken cancellationToken = default);
        IAsyncEnumerable<WorkerMessage> ReadAllAsync(CancellationToken cancellationToken);
    }
}
