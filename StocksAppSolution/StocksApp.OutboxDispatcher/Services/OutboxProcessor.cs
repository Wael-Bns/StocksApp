using Microsoft.Extensions.Options;
using StocksApp.Core.Exceptions;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Notifications;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.OutboxDispatcher.Options;

namespace StocksApp.OutboxDispatcher.Services
{
    public class OutboxProcessor : IOutboxProcessor
    {
        private readonly Dictionary<string,IOutboxEventHandler> _eventHandlers;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly IOutboxRepository _outboxRepository;
        private readonly OutboxOptions _options;
        public OutboxProcessor(IEnumerable<IOutboxEventHandler> handlers,
            ILogger<OutboxProcessor> logger,
            IOutboxRepository outboxRepository,
            IOptions<OutboxOptions> options)
        {
            _eventHandlers = handlers.ToDictionary(h => h.EventType);
            _logger = logger;
            _outboxRepository = outboxRepository;
            _options = options.Value;
        }

        private async Task HandleMessage(string eventName, string payload, Guid outboxId, int retryCount)
        {
            if (!_eventHandlers.TryGetValue(eventName, out var handler))
            {
                _logger.LogError("No handler registered for {EventType}", eventName);
                return;
            }

            try
            {
                await handler.HandleAsync(payload);
                await _outboxRepository.MarkAsProcessed(outboxId);
            }
            catch (OutboxDeserializationException ex)
            {
                await _outboxRepository.MarkAsFailed(outboxId, ex.Message);
                _logger.LogCritical(ex, "Outbox event {OutboxId} has an unparseable payload — dead-lettered immediately", outboxId);
            }
            catch (Exception ex)
            {
                await _outboxRepository.RecordFailure(
                    outboxId,
                    ex.Message,
                    maxRetries: _options.MaxRetries,
                    backoff: ComputeBackoff(retryCount));

                _logger.LogError(ex, "Failed processing outbox event {OutboxId} (attempt {RetryCount})", outboxId, retryCount);
            }
        }
        private static TimeSpan ComputeBackoff(int retryCount)
        {
            var seconds = Math.Min(Math.Pow(2, retryCount) * 5, 300);// cap at 5 min
            var jitter = Random.Shared.NextDouble() * 0.3 * seconds;
            return TimeSpan.FromSeconds(seconds + jitter);
        }
        public async Task ProcessNotificationAsync(OutboxNotification notification)
        {
            string payload = notification.Payload.GetRawText();
            await HandleMessage(notification.EventName, payload, notification.OutboxId, 0);
        }

        public async Task ProcessUnprocessedEvents(List<Outbox> unprocessedEvents)
        {
            foreach (var evt in unprocessedEvents)
            {
               await HandleMessage(evt.EventName, evt.Payload, evt.OutboxId, evt.RetryCount);       
            }
        }
    }
}
