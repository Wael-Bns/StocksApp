using MassTransit;
using RabbitMQ.Client;
using StocksApp.Domain.Events;
using StocksApp.Infrastructure.Helpers;

namespace StocksApp.Infrastructure.MessageBroker.Profiles
{
    public class PriceTickPublishedEventBusProfile : IEventBusProfile
    {
        public void ConfigureMessages(IRabbitMqBusFactoryConfigurator cfg)
        {
            cfg.Message<IPriceTickPublished>(x => x.SetEntityName(RabbitMQExchanges.PricesExchange));
            cfg.Publish<IPriceTickPublished>(x => x.ExchangeType = ExchangeType.Topic);
        }
    }
}
