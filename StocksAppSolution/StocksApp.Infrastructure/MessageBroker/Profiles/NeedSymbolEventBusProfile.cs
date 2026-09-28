using MassTransit;
using StocksApp.Domain.Events;
using StocksApp.Infrastructure.Helpers;

namespace StocksApp.Infrastructure.MessageBroker.Profiles
{
    public class NeedSymbolEventBusProfile : IEventBusProfile
    {
        public void ConfigureMessages(IRabbitMqBusFactoryConfigurator cfg)
        {
            cfg.Message<INeedSymbol>(x => x.SetEntityName(RabbitMQExchanges.NeedSymbolExchange));
        }
    }
}
