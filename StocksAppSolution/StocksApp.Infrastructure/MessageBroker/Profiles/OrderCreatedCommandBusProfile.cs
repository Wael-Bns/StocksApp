using MassTransit;
using StocksApp.Domain.Events;
using StocksApp.Infrastructure.Helpers;

namespace StocksApp.Infrastructure.MessageBroker.Profiles
{
    public class OrderCreatedCommandBusProfile : ICommandBusProfile
    {
        public Type MessageType => typeof(SellOrderCreatedCommand);
        public Uri EndpointUri => new Uri($"exchange:{RabbitMQExchanges.OrdersExchange}?type=direct");
        public void ConfigureMessages(IRabbitMqBusFactoryConfigurator cfg)
        {
            cfg.Message<SellOrderCreatedCommand>(x => x.SetEntityName(RabbitMQExchanges.OrdersExchange));
            
            cfg.Send<SellOrderCreatedCommand>(x => x.UseRoutingKeyFormatter(_ => "sellorder.created"));

            cfg.Publish<SellOrderCreatedCommand>(x => { x.ExchangeType = "direct";  });
        }
    }
}
