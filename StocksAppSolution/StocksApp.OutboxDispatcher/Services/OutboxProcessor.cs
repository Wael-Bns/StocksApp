using StocksApp.Core.Exceptions;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Notifications;
using StocksApp.Domain.RepositoryContracts;

namespace StocksApp.OutboxDispatcher.Services
{
    public class OutboxProcessor : IOutboxProcessor
    {
        private readonly Dictionary<string,IOutboxEventHandler> _handlers;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly IOutboxRepository _outboxRepository;
        public OutboxProcessor(IEnumerable<IOutboxEventHandler> handlers,ILogger<OutboxProcessor> logger , IOutboxRepository outboxRepository)
        {
            _handlers = handlers.ToDictionary(h => h.EventType);
            _logger = logger;
            _outboxRepository = outboxRepository;
        }

        public async Task PublishNotificationAsync(OutboxNotification notification)
        {
            try
            {
                if(!_handlers.TryGetValue(notification.EventType, out var handler))
                {
                    _logger.LogError("No handler registered for {EventType}", notification.EventType);
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
                    if(!_handlers.TryGetValue(unprocessedEvent.EventType.AssemblyQualifiedName!,out var handler))
                    {
                        _logger.LogError("No handler registered for {EventType}", unprocessedEvent.EventType.AssemblyQualifiedName);
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
