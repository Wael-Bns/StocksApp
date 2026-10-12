using System.Threading.Channels;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StocksApp.Core.Diagnostics;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Events;

namespace StocksApp.PriceFeed.BackgroundServices
{
    public sealed class TickPublisherService : BackgroundService
    {
        private readonly ChannelReader<PriceUpdateMessage> _reader;
        private readonly IBus _bus;
        private readonly IOhlcBarAggregator _chartAggregator;
        private readonly IOhlcBarAggregator _matchAggregator;
        private readonly ILatestPriceCacheWriter _latestPriceCacheWriter;
        private readonly IPriceFeedMetrics _metrics;
        private readonly CandleCacheOptions _candleOptions;
        private readonly OrderMatchingOptions _matchingOptions;
        private readonly ILogger<TickPublisherService> _logger;

        public TickPublisherService(
            ChannelReader<PriceUpdateMessage> reader,
            IBus bus,
            [FromKeyedServices("chart")] IOhlcBarAggregator chartAggregator,
            [FromKeyedServices("match")] IOhlcBarAggregator matchAggregator,
            ILatestPriceCacheWriter latestPriceCacheWriter,
            IPriceFeedMetrics metrics,
            IOptions<CandleCacheOptions> candleOptions,
            IOptions<OrderMatchingOptions> matchingOptions,
            ILogger<TickPublisherService> logger)
        {
            _reader = reader;
            _bus = bus;
            _chartAggregator = chartAggregator;
            _matchAggregator = matchAggregator;
            _latestPriceCacheWriter = latestPriceCacheWriter;
            _metrics = metrics;
            _candleOptions = candleOptions.Value;
            _matchingOptions = matchingOptions.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var update in _reader.ReadAllAsync(stoppingToken))
            {
                if (await PublishTickAsync(update, stoppingToken))
                    break;

                if (await RunCandleWorkAsync(update, stoppingToken))
                    break;
            }
        }

        private async Task<bool> RunCandleWorkAsync(PriceUpdateMessage update, CancellationToken stoppingToken)
        {
            var pending = new List<Task<bool>>(3);

            if (_candleOptions.Enabled)
            {
                pending.Add(AggregateCandleAsync(_chartAggregator, "chart", update, stoppingToken));
                pending.Add(WriteLatestPriceAsync(update, stoppingToken));
            }

            if (_matchingOptions.Enabled)
                pending.Add(AggregateCandleAsync(_matchAggregator, "match", update, stoppingToken));

            if (pending.Count == 0)
                return false;

            var results = await Task.WhenAll(pending);
            return Array.Exists(results, shouldStop => shouldStop);
        }

        private async Task<bool> PublishTickAsync(PriceUpdateMessage update, CancellationToken stoppingToken)
        {
            try
            {
                await _bus.Publish<IPriceTickPublished>(
                    new PriceTickPublished(
                        update.StockSymbol, update.Price, update.Volume,
                        DateTimeOffset.FromUnixTimeMilliseconds(update.Timestamp)),
                    ctx => ctx.SetRoutingKey(update.StockSymbol),
                    stoppingToken);
                _metrics.TickPublished();
                return false;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish price tick for {Symbol}", update.StockSymbol);
                return false;
            }
        }

        private async Task<bool> AggregateCandleAsync(
            IOhlcBarAggregator aggregator, string label, PriceUpdateMessage update, CancellationToken stoppingToken)
        {
            try
            {
                await aggregator.ApplyTickAsync(update, stoppingToken);
                return false;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to aggregate {Label} candle for {Symbol}", label, update.StockSymbol);
                return false;
            }
        }

        private async Task<bool> WriteLatestPriceAsync(PriceUpdateMessage update, CancellationToken stoppingToken)
        {
            try
            {
                await _latestPriceCacheWriter.WriteAsync(update, stoppingToken);
                return false;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to write latest price for {Symbol}", update.StockSymbol);
                return false;
            }
        }
    }
}