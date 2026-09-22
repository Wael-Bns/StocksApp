using System.Text.Json;
using StocksApp.Core.Exceptions;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Events;

namespace StocksApp.OutboxDispatcher.EventHandlers
{
    public abstract class OutboxEventHandlerBase<TEvent> : IOutboxEventHandler
    where TEvent : IOutboxEvent
    {
        public string EventType => TEvent.EventName;

        public async Task HandleAsync(string payload)
        {
            var @event = JsonSerializer.Deserialize<TEvent>(payload)
                ?? throw new OutboxDeserializationException(typeof(TEvent), payload);

            await HandleEventAsync(@event);
        }

        protected abstract Task HandleEventAsync(TEvent @event);
    }
}
