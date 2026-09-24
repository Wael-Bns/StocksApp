using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Events;

namespace StocksApp.WebApi.HostedServices
{
    public sealed class StockPricesHostedService : BackgroundService
    {
        private readonly ILogger<StockPricesHostedService> _logger;
        private readonly IPriceFeedSubscriber _priceFeedSubscriber;
        private readonly IPriceTickNotifier _notifier;

        public StockPricesHostedService(
            ILogger<StockPricesHostedService> logger,
            IPriceFeedSubscriber priceFeedSubscriber,
            IPriceTickNotifier notifier)
        {
            _logger = logger;
            _priceFeedSubscriber = priceFeedSubscriber;
            _notifier = notifier;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting {ServiceName}...", nameof(StockPricesHostedService));
            _priceFeedSubscriber.OnPriceTick += OnPriceTickAsync;

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("{ServiceName} is stopping due to cancellation.", nameof(StockPricesHostedService));
            }
            finally
            {
                _priceFeedSubscriber.OnPriceTick -= OnPriceTickAsync;
                _logger.LogInformation("{ServiceName} stopped.", nameof(StockPricesHostedService));
            }
        }

        private Task OnPriceTickAsync(IPriceTickPublished priceTick, CancellationToken ct)
            => _notifier.NotifyAsync(priceTick, ct);
    }
}