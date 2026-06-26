using MassTransit;
using StocksApp.Infrastructure.Helpers;
using StocksApp.OrdersWorker.MessageBroker;

namespace StocksApp.OrdersWorker.IoC
{
    public static class MassTransitExtensions
    {
        public static IServiceCollection AddRabbitMqConsumers(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var settings = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>()
                ?? throw new InvalidOperationException("RabbitMQ settings are not configured.");

            services.AddMassTransit(x =>
            {
                x.AddConsumer<SellOrderCreatedConsumer>();

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(settings.HostName, "/", h =>
                    {
                        h.Username(settings.UserName!);
                        h.Password(settings.Password!);
                    });

                    cfg.ReceiveEndpoint(RabbitMQQueues.SellOrderCreatedQueue, e =>
                    {
                        e.Bind(RabbitMQExchanges.OrdersExchange, b =>
                        {
                            b.ExchangeType = "direct";
                            b.RoutingKey = "sellorder.created";
                        });

                        e.ConfigureConsumer<SellOrderCreatedConsumer>(ctx);
                    });
                });
            });

            return services;
        }
    }
}
