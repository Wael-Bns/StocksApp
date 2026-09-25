using MassTransit;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Events;

namespace StocksApp.PriceFeed.Consumers
{
    public class NeedSymbolConsumer : IConsumer<INeedSymbol>
    {
        private readonly ISymbolRegistry _registry;
        private readonly ILogger<NeedSymbolConsumer> _logger;

        public NeedSymbolConsumer(ISymbolRegistry registry, ILogger<NeedSymbolConsumer> logger)
        {
            _registry = registry;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<INeedSymbol> context)
        {
            await _registry.AddInterestAsync(context.Message.Symbol, context.CancellationToken);
            _logger.LogInformation("Consumed NeedSymbol {Symbol}", context.Message.Symbol);
        }
    }
}
