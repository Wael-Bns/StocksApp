using System.Text.Json;
using StocksApp.Core.Exceptions;
using StocksApp.Core.MessageBroker.Publisher;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Notifications;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;

namespace StocksApp.OutboxDispatcher.Services
{
    public class OutboxProcessor : IOutboxProcessor
    {
        private readonly Dictionary<string,IOutboxEventHandler> _handlers;
        private readonly IOutboxRepository _outboxRepository;
        public OutboxProcessor(IEnumerable<IOutboxEventHandler> handlers, IOutboxRepository outboxRepository)
        {
            _handlers = handlers.ToDictionary(h => h.EventType);
            _outboxRepository = outboxRepository;
        }

        public async Task PublishNotificationAsync(OutboxNotification notification)
        {
            if(!_handlers.TryGetValue(notification.EventType, out var handler))
            {
                throw new InvalidOutboxEventTypeException();            
            }
            await handler.HandleAsync(notification.Payload);
            await _outboxRepository.MarkAsProcessed(Guid.Parse(notification.OutboxId));
        }

        public async Task PublishUnprocessedEvents(List<Outbox> unprocessedEvents)
        {
            foreach (var unprocessedEvent in unprocessedEvents)
            {
                if(!_handlers.TryGetValue(nameof(unprocessedEvent.EventType),out var handler))
                {
                    throw new InvalidOutboxEventTypeException();
                }
                await handler.HandleAsync(unprocessedEvent.Payload);
            }
        }
    }
}
