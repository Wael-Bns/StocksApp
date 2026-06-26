using StocksApp.OrdersWorker.Messages;

namespace StocksApp.OrdersWorker.MessageHandlers
{
    public interface IWorkerMessageHandler<TMessage> where TMessage : WorkerMessage
    {
        Task HandleAsync(TMessage message, CancellationToken cancellationToken = default);
    }
}
