using System.Threading.Channels;
using MassTransit;
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
        private readonly IOhlcBarAggregator _aggregator;
        private readonly ILatestPriceCacheWriter _latestPriceCacheWriter;
        private readonly IPriceFeedMetrics _metrics;
        private readonly CandleCacheOptions _candleOptions;
        private readonly ILogger<TickPublisherService> _logger;

        public TickPublisherService(
            ChannelReader<PriceUpdateMessage> reader,
            IBus bus,
            IOhlcBarAggregator aggregator,
            ILatestPriceCacheWriter latestPriceCacheWriter,
            IPriceFeedMetrics metrics,
            IOptions<CandleCacheOptions> candleOptions,
            ILogger<TickPublisherService> logger)
        {
            _reader = reader;
            _bus = bus;
            _aggregator = aggregator;
            _latestPriceCacheWriter = latestPriceCacheWriter;
            _metrics = metrics;
            _candleOptions = candleOptions.Value;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var update in _reader.ReadAllAsync(stoppingToken))
            {
                if (await PublishTickAsync(update, stoppingToken))
                    break;

                if (_candleOptions.Enabled)
                {
                    var writingToCacheTask = WriteLatestPriceAsync(update, stoppingToken);
                    var aggregatingCandleTask = AggregateCandleAsync(update, stoppingToken);
                    bool terminateWriting = await writingToCacheTask;
                    bool terminateAggregating = await aggregatingCandleTask;
                    if(terminateAggregating || terminateWriting)
                    {
                        return;
                    }
                }
            }
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

        private async Task<bool> AggregateCandleAsync(PriceUpdateMessage update, CancellationToken stoppingToken)
        {
            try
            {
                await _aggregator.ApplyTickAsync(update, stoppingToken);
                return false;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to aggregate candle for {Symbol}", update.StockSymbol);
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