using MassTransit;
using StocksApp.Domain.Events;
using StocksApp.Infrastructure.Helpers;

namespace StocksApp.Infrastructure.MessageBroker.Profiles
{
    public class ReleaseSymbolEventBusProfile : IEventBusProfile
    {
        public void ConfigureMessages(IRabbitMqBusFactoryConfigurator cfg)
        {
            cfg.Message<IReleaseSymbol>(x => x.SetEntityName(RabbitMQExchanges.ReleaseSymbolExchange));
        }
    }
}
