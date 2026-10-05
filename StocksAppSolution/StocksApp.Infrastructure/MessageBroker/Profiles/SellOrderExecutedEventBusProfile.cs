using MassTransit;
using RabbitMQ.Client;
using StocksApp.Domain.Events;

namespace StocksApp.Infrastructure.MessageBroker.Profiles
{
    public class SellOrderExecutedEventBusProfile : IEventBusProfile
    {
        public void ConfigureMessages(IRabbitMqBusFactoryConfigurator cfg)
        {
            cfg.Message<SellOrderExecuted>(m => m.SetEntityName("sell-order-executed"));
            cfg.Publish<SellOrderExecuted>(p => p.ExchangeType = ExchangeType.Topic);
        }
    }
}