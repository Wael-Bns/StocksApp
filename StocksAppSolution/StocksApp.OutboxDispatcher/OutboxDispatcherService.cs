using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Notifications;
using StocksApp.Domain.RepositoryContracts;

namespace StocksApp.OutboxDispatcher
{
    public class OutboxDispatcherService : BackgroundService
    {
        private readonly ILogger<OutboxDispatcherService> _logger;
        private readonly IOutboxNotificationsListener _listener;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OutboxDispatcherService(ILogger<OutboxDispatcherService> logger, IOutboxNotificationsListener outboxNotificationsListener, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _listener = outboxNotificationsListener;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("OutboxDispatcherService is starting.");
                _listener.OnNotificationReceived += ProcessNotificationReceived;
                _listener.OnPeriodicChecks += ProcessPeriodicChecks;
                await _listener.ListenAsync(stoppingToken);
            }
            finally
            {
                _logger.LogInformation("OutboxDispatcherService is stopping.");
                _listener.OnNotificationReceived -= ProcessNotificationReceived;
                _listener.OnPeriodicChecks -= ProcessPeriodicChecks;
            }
        }
        private async Task ProcessNotificationReceived(OutboxNotification notification)
        {
            _logger.LogInformation("Received notification: {Notification}", notification);
            using var scope = _serviceScopeFactory.CreateScope();
            var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
            await outboxProcessor.PublishNotificationAsync(notification);
        }
        private async Task ProcessPeriodicChecks()
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
            
            var unprocessedEvents = await outboxRepository.GetUnprocessedEvents();
            
            await outboxProcessor.PublishUnprocessedEvents(unprocessedEvents);
        }
    }
}
