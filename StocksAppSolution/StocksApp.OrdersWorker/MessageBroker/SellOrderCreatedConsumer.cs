using MassTransit;
using StocksApp.Domain.Events;
using StocksApp.OrdersWorker.Channels;
using StocksApp.OrdersWorker.Messages;

namespace StocksApp.OrdersWorker.MessageBroker
{
    public class SellOrderCreatedConsumer : IConsumer<SellOrderCreatedCommand>
    {
        private readonly IWorkerChannel _channel;
        private readonly ILogger<SellOrderCreatedConsumer> _logger;
        public SellOrderCreatedConsumer(IWorkerChannel channel, ILogger<SellOrderCreatedConsumer> logger)
        {
            _channel = channel;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<SellOrderCreatedCommand> context)
        {
            await _channel.EnqueueAsync(new SellOrderCreatedWorkerMessage(context.Message));
            _logger.LogInformation("Consumed SellOrderCreatedCommand {OrderId}", context.Message.SellOrderId);
        }
    }
}
