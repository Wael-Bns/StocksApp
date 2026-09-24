using MassTransit;
using StocksApp.Infrastructure.Helpers;
using StocksApp.Infrastructure.Options;
using StocksApp.PriceFeed.Consumers;

namespace StocksApp.PriceFeed.IoC
{
    public static class MassTransitExtensions
    {
        public static IServiceCollection AddRabbitMqConsumers(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var settings = configuration.GetSection("RabbitMQ").Get<RabbitMqOptions>()
                ?? throw new InvalidOperationException("RabbitMQ settings are not configured.");

            services.AddMassTransit(x =>
            {
                x.AddConsumer<NeedSymbolConsumer>();
                x.AddConsumer<ReleaseSymbolConsumer>();

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(settings.HostName, "/", h =>
                    {
                        h.Username(settings.UserName!);
                        h.Password(settings.Password!);
                    });

                    cfg.ReceiveEndpoint(RabbitMQQueues.NeedSymbolQueue, e =>
                    {
                        e.ConfigureConsumer<NeedSymbolConsumer>(ctx);
                    });

                    cfg.ReceiveEndpoint(RabbitMQQueues.ReleaseSymbolQueue, e =>
                    {
                        e.ConfigureConsumer<ReleaseSymbolConsumer>(ctx);
                    });
                });
            });

            return services;
        }
    }
}
