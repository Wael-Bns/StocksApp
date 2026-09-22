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

        public async Task ProcessNotificationAsync(OutboxNotification notification)
        {
            try
            {
                if(!_eventHandlers.TryGetValue(notification.EventName, out var handler))
                {
                    _logger.LogError("No handler registered for {EventType}", notification.EventName);
                    return;
                }
                await handler.HandleAsync(notification.Payload.GetRawText());
                await _outboxRepository.MarkAsProcessed(notification.OutboxId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed processing outbox notification {OutboxId}", notification.OutboxId);
            }
        }

        public async Task ProcessUnprocessedEvents(List<Outbox> unprocessedEvents)
        {
            foreach (var unprocessedEvent in unprocessedEvents)
            {
                try
                {
                    if(!_eventHandlers.TryGetValue(unprocessedEvent.EventName,out var handler))
                    {
                        _logger.LogError("No handler registered for {EventType}", unprocessedEvent.EventName);
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
