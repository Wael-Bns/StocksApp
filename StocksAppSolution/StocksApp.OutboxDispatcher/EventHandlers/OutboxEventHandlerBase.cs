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
            try
            {
                var @event = JsonSerializer.Deserialize<TEvent>(payload)
                    ?? throw new JsonException($"Deserialized {typeof(TEvent).Name} payload was null.");
                await HandleEventAsync(@event);
            }
            catch(JsonException)
            {
                throw;
            }
        }

        protected abstract Task HandleEventAsync(TEvent @event);
    }
}
