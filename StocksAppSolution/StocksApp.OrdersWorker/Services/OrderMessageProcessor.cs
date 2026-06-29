using StocksApp.OrdersWorker.Channels;
using StocksApp.OrdersWorker.MessageHandlers;
using StocksApp.OrdersWorker.Messages;
using StocksApp.OrdersWorker.ServiceContracts;

namespace StocksApp.OrdersWorker.Services
{
    public sealed class OrderMessageProcessor : IOrderMessageProcessor
    {
        private readonly IWorkerChannel _channel;
        private readonly IReadOnlyDictionary<Type, IWorkerMessageHandler> _handlers;
        private readonly ILogger<OrderMessageProcessor> _logger;

        public OrderMessageProcessor(
            IWorkerChannel channel,
            IEnumerable<IWorkerMessageHandler> handlers,
            ILogger<OrderMessageProcessor> logger)
        {
            _channel = channel;
            _handlers = handlers.ToDictionary(h => h.MessageType);
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await foreach (var message in _channel.ReadAllAsync(cancellationToken))
            {
                try
                {
                    await DispatchAsync(message, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error processing message {MessageType}", message.GetType().Name);
                }
            }
        }

        private async Task DispatchAsync(WorkerMessage message, CancellationToken cancellationToken)
        {
            if (!_handlers.TryGetValue(message.GetType(), out var handler))
                throw new InvalidOperationException($"No handler registered for {message.GetType().Name}");

            await handler.HandleAsync(message, cancellationToken);
        }
    }
}
