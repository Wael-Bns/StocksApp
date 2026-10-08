using MassTransit;
using RabbitMQ.Client;
using StocksApp.Domain.Events;
using StocksApp.Infrastructure.Helpers;

namespace StocksApp.Infrastructure.MessageBroker.Profiles
{
    public class SellOrderExecutedEventBusProfile : IEventBusProfile
    {
        public void ConfigureMessages(IRabbitMqBusFactoryConfigurator cfg)
        {
            cfg.Message<SellOrderExecuted>(m => m.SetEntityName(RabbitMQExchanges.SellOrderExecutedExchange));
            cfg.Publish<SellOrderExecuted>(p => p.ExchangeType = ExchangeType.Topic);
        }
    }
}