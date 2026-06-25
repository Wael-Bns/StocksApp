using MassTransit;

namespace StocksApp.Infrastructure.MessageBroker.Profiles
{
    public interface ICommandBusProfile
    {
        void ConfigureMessages(IRabbitMqBusFactoryConfigurator cfg);
    }
}
