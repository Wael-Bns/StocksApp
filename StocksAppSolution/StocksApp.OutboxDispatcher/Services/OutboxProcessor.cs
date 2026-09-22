using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Notifications;
using StocksApp.Domain.RepositoryContracts;

namespace StocksApp.OutboxDispatcher.Services
{
    public class OutboxProcessor : IOutboxProcessor
    {
        private readonly Dictionary<string,IOutboxEventHandler> _eventHandlers;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly IOutboxRepository _outboxRepository;
        public OutboxProcessor(IEnumerable<IOutboxEventHandler> handlers,ILogger<OutboxProcessor> logger , IOutboxRepository outboxRepository)
        {
            _eventHandlers = handlers.ToDictionary(h => h.EventType);
            _logger = logger;
            _outboxRepository = outboxRepository;
        }

        public async Task PublishNotificationAsync(OutboxNotification notification)
        {
            try
            {
                if(!_eventHandlers.TryGetValue(notification.Event, out var handler))
                {
                    _logger.LogError("No handler registered for {EventType}", notification.Event);
                    return;
                }
                await handler.HandleAsync(notification.Payload.GetRawText());
                await _outboxRepository.MarkAsProcessed(Guid.Parse(notification.OutboxId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed processing outbox notification {OutboxId}", notification.OutboxId);
            }
        }

        public async Task PublishUnprocessedEvents(List<Outbox> unprocessedEvents)
        {
            foreach (var unprocessedEvent in unprocessedEvents)
            {
                try
                {
                    if(!_eventHandlers.TryGetValue(unprocessedEvent.Event,out var handler))
                    {
                        _logger.LogError("No handler registered for {EventType}", unprocessedEvent.Event);
                        continue;
                    }
                    await handler.HandleAsync(unprocessedEvent.Payload);
                    await _outboxRepository.MarkAsProcessed(unprocessedEvent.OutboxId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed processing outbox event {OutboxId}", unprocessedEvent.OutboxId);
                }
            }
        }
    }
}
