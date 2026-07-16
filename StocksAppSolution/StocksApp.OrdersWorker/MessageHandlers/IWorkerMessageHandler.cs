using StocksApp.OrdersWorker.Messages;

namespace StocksApp.OrdersWorker.MessageHandlers
{
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
