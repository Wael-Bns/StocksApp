using StocksApp.Core.MessageBroker.Publisher;
using StocksApp.Domain.Events;

namespace StocksApp.OutboxDispatcher.EventHandlers
{
    public class SellOrderExecutedOutboxHandler : OutboxEventHandlerBase<SellOrderExecuted>
    {
        private readonly IEventPublisher _eventPublisher;
        public SellOrderExecutedOutboxHandler(IEventPublisher eventPublisher) => _eventPublisher = eventPublisher;

        protected override Task HandleEventAsync(SellOrderExecuted @event) =>
            _eventPublisher.PublishAsync(@event);
    }
}