using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Notifications;

namespace StocksApp.OutboxDispatcher.BackgroundServices
{
    public class OutboxDispatcherService : BackgroundService
    {
        private readonly ILogger<OutboxDispatcherService> _logger;
        private readonly IOutboxNotificationsListener _listener;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OutboxDispatcherService(
            ILogger<OutboxDispatcherService> logger,
            IOutboxNotificationsListener listener,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _listener = listener;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("OutboxDispatcherService is starting...");
                _listener.OnNotificationReceived += ProcessNotificationReceived;
                await _listener.ListenAsync(stoppingToken);
            }
            finally
            {
                _logger.LogInformation("OutboxDispatcherService is stopping...");
                _listener.OnNotificationReceived -= ProcessNotificationReceived;
            }
        }

        private async Task ProcessNotificationReceived(OutboxNotification notification)
        {
            _logger.LogInformation("Received notification: {Notification}", notification);
            using var scope = _serviceScopeFactory.CreateScope();
            var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
            await outboxProcessor.PublishNotificationAsync(notification);
        }
    }
}
