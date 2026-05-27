using System.Numerics;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class StocksProcessor : IStocksProcessor
    {
        private readonly Channel<PriceUpdateMessage> _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StocksProcessor> _logger;
        public StocksProcessor(IServiceProvider serviceProvider, ILogger<StocksProcessor> logger)
        {
            _channel = Channel.CreateBounded<PriceUpdateMessage>(new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        public async Task EnqueueMessageAsync(PriceUpdateMessage priceUpdateMessage)
        {
            await _channel.Writer.WriteAsync(priceUpdateMessage);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await foreach (var message in _channel.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var executor = scope.ServiceProvider.GetRequiredService<IOrdersExecutor>();

                        await executor.ExecuteSellOrders(message.StockSymbol, message.Price);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error processing price update for {StockSymbol} at price {Price}",
                            message.StockSymbol,
                            message.Price);
                    }
                }
            }
            finally
            {
                _channel.Writer.TryComplete();
            }
        }
    }
}
