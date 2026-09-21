using StocksApp.OrdersWorker.Messages;

namespace StocksApp.OrdersWorker.MessageHandlers
{
    /// <summary>
    /// Define the contract every message handler follows, so messages can be routed by type.
    /// </summary>
    public interface IWorkerMessageHandler
    {
        Type MessageType { get; }
        Task HandleAsync(WorkerMessage message, CancellationToken cancellationToken);
    }
    public abstract class WorkerMessageHandler<T> : IWorkerMessageHandler where T : WorkerMessage
    {
        public Type MessageType => typeof(T);

        public Task HandleAsync(WorkerMessage message, CancellationToken ct)
        {
            return HandleAsync((T)message, ct);
        }

        protected abstract Task HandleAsync(T message, CancellationToken ct);
    }
}
