
using StocksApp.Core.ServiceContracts;

namespace StocksApp.WebApi.HostedServices
{
    public class OrdersWorker : BackgroundService
    {
        private readonly ILogger<OrdersWorker> _logger;
        private readonly IStocksProcessor _stocksProcessor;
        public OrdersWorker(ILogger<OrdersWorker> logger, IStocksProcessor stocksProcessor)
        {
            _logger = logger;
            _stocksProcessor = stocksProcessor;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting {ServiceName}...", nameof(OrdersWorker));
            await _stocksProcessor.StartAsync(stoppingToken);
        }
    }
}
