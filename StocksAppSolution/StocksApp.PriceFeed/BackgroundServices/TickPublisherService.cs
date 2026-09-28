using System.Threading.Channels;
using MassTransit;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Domain.Events;
using StocksApp.PriceFeed.Diagnostics;

public sealed class TickPublisherService : BackgroundService
{
    private readonly ChannelReader<PriceUpdateMessage> _reader;
    private readonly IBus _bus;
    private readonly PriceFeedMetrics _metrics;
    private readonly ILogger<TickPublisherService> _logger;

    public TickPublisherService(
        ChannelReader<PriceUpdateMessage> reader,
        IBus bus,
        PriceFeedMetrics metrics,
        ILogger<TickPublisherService> logger)
    {
        _reader = reader;
        _bus = bus;
        _metrics = metrics;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var update in _reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _bus.Publish<IPriceTickPublished>(
                    new PriceTickPublished(
                        update.StockSymbol,
                        update.Price,
                        update.Volume,
                        DateTimeOffset.FromUnixTimeMilliseconds(update.Timestamp)),
                    ctx => ctx.SetRoutingKey(update.StockSymbol),
                    stoppingToken);

                _metrics.TickPublished();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish price tick for {Symbol}", update.StockSymbol);
            }
        }
    }
}