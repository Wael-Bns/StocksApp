using System.Threading.Channels;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Domain.Events;
using MassTransit;

namespace StocksApp.PriceFeed.BackgroundServices
{
    public sealed class TickPublisherService : BackgroundService
    {
        private readonly ChannelReader<PriceUpdateMessage> _reader;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TickPublisherService> _logger;

        public TickPublisherService(
            ChannelReader<PriceUpdateMessage> reader,
            IServiceScopeFactory scopeFactory,
            ILogger<TickPublisherService> logger)
        {
            _reader = reader;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var update in _reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                    await publishEndpoint.Publish<IPriceTickPublished>(
                        new PriceTickPublished(
                            update.StockSymbol,
                            update.Price,
                            update.Volume,
                            DateTimeOffset.FromUnixTimeMilliseconds(update.Timestamp)),
                        ctx => ctx.SetRoutingKey(update.StockSymbol),
                        stoppingToken);
                }
                catch (Exception ex)
                {
                    // A dropped/delayed tick isn't worth crashing the publisher loop over —
                    // the next tick for this symbol is seconds away.
                    _logger.LogWarning(ex, "Failed to publish price tick for {Symbol}", update.StockSymbol);
                }
            }
        }
    }
}