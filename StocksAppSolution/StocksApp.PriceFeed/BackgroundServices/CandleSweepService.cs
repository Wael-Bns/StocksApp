using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.PriceFeed.BackgroundServices
{
    /// <summary>
    /// Closes a bar with no triggering next tick (end of day, halted, delisted symbol).
    /// Runs on every instance, leader or standby — on a standby, _states is always empty,
    /// </summary>
    public sealed class CandleSweepService : BackgroundService
    {
        private readonly IOhlcBarAggregator _aggregator;
        private readonly CandleCacheOptions _options;
        private readonly ILogger<CandleSweepService> _logger;

        public CandleSweepService(
            IOhlcBarAggregator aggregator,
            IOptions<CandleCacheOptions> options,
            ILogger<CandleSweepService> logger)
        {
            _aggregator = aggregator;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
                return;   // feature flag off — no Redis/Postgres traffic from this service at all

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_options.SweepInterval, stoppingToken);
                    await _aggregator.FlushIdleBarsAsync(DateTimeOffset.UtcNow, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Idle-bar sweep pass failed; will retry next interval.");
                }
            }
        }
    }
}