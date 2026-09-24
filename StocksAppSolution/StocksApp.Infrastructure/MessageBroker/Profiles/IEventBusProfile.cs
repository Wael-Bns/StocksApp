using MassTransit;

namespace StocksApp.Infrastructure.MessageBroker.Profiles
{
    public interface IEventBusProfile
    {
        void ConfigureMessages(IRabbitMqBusFactoryConfigurator cfg);
    }
}
