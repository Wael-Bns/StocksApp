using MassTransit;
using Microsoft.Extensions.Logging;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Events;

namespace StocksApp.Infrastructure.MessageBroker.Consumers
{
    public class ReleaseSymbolConsumer : IConsumer<IReleaseSymbol>
    {
        private readonly ISymbolRegistry _registry;
        private readonly ILogger<ReleaseSymbolConsumer> _logger;

        public ReleaseSymbolConsumer(ISymbolRegistry registry, ILogger<ReleaseSymbolConsumer> logger)
        {
            _registry = registry;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IReleaseSymbol> context)
        {
            await _registry.RemoveInterestAsync(context.Message.Symbol, context.CancellationToken);
            _logger.LogInformation("Consumed ReleaseSymbol {Symbol}", context.Message.Symbol);
        }
    }
}
