using System.Threading.Channels;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.OrdersWorker.ServiceContracts;

namespace StocksApp.OrdersWorker.Services
{
    internal class PriceUpdateOrderProcessor : IPriceUpdateOrderProcessor
    {
        private readonly Channel<PriceUpdateMessage> _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PriceUpdateOrderProcessor> _logger;
        public PriceUpdateOrderProcessor(IServiceProvider serviceProvider, ILogger<PriceUpdateOrderProcessor> logger)
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
