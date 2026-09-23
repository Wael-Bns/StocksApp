using Microsoft.Extensions.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.OutboxDispatcher.Options;

namespace StocksApp.OutboxDispatcher.BackgroundServices
{
    public class OutboxPollingBackgroundService : BackgroundService
    {
        private readonly ILogger<OutboxPollingBackgroundService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly OutboxOptions _options;

        public OutboxPollingBackgroundService(
            ILogger<OutboxPollingBackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<OutboxOptions> options)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(_options.FallbackPollInterval);

            _logger.LogInformation("Outbox polling started with interval {Interval}", _options.FallbackPollInterval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await PollOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Outbox poll cycle failed");
                }
            }
        }

        private async Task PollOnceAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var outboxProcessor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();

            var unprocessedEvents = await outboxRepository.GetUnprocessedEvents();
            await outboxProcessor.ProcessUnprocessedEvents(unprocessedEvents);
        }
    }
}
