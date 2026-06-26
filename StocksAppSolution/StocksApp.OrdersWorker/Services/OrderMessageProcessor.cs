using StocksApp.OrdersWorker.Channels;
using StocksApp.OrdersWorker.MessageHandlers;
using StocksApp.OrdersWorker.Messages;
using StocksApp.OrdersWorker.ServiceContracts;

namespace StocksApp.OrdersWorker.Services
{
    public sealed class OrderMessageProcessor : IOrderMessageProcessor
    {
        private readonly IWorkerChannel _channel;
        private readonly IWorkerMessageHandler<PriceUpdateWorkerMessage> _priceUpdateHandler;
        private readonly IWorkerMessageHandler<SellOrderCreatedWorkerMessage> _sellOrderHandler;
        private readonly ILogger<OrderMessageProcessor> _logger;

        public OrderMessageProcessor(
            IWorkerChannel channel,
            IWorkerMessageHandler<PriceUpdateWorkerMessage> priceUpdateHandler,
            IWorkerMessageHandler<SellOrderCreatedWorkerMessage> sellOrderHandler,
            ILogger<OrderMessageProcessor> logger)
        {
            _channel = channel;
            _priceUpdateHandler = priceUpdateHandler;
            _sellOrderHandler = sellOrderHandler;
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

        private Task DispatchAsync(WorkerMessage message, CancellationToken cancellationToken)
        {
            return message switch
            {
                PriceUpdateWorkerMessage m => _priceUpdateHandler.HandleAsync(m, cancellationToken),
                SellOrderCreatedWorkerMessage m => _sellOrderHandler.HandleAsync(m, cancellationToken),
                _ => throw new InvalidOperationException($"Unknown message type: {message.GetType().Name}")
            };
        }
    }
}
