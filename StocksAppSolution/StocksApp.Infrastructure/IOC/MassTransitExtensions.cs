using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.MessageBroker.Publisher;
using StocksApp.Infrastructure.Helpers;
using StocksApp.Infrastructure.MessageBroker;
using StocksApp.Infrastructure.MessageBroker.Consumers;
using StocksApp.Infrastructure.MessageBroker.Profiles;
using StocksApp.Infrastructure.Options;

namespace StocksApp.Infrastructure.IoC
{
    public static class MassTransitExtensions
    {
        public static IServiceCollection AddInfrastructureMessaging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var settings = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>()
                ?? throw new InvalidOperationException("RabbitMQ settings are not configured.");

            // Register profiles
            services.AddCommandBusProfiles();
            services.AddEventBusProfiles();

            // SINGLE MassTransit registration
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

                    // Configure Producer/Publish Profiles
                    foreach (var profile in ctx.GetServices<ICommandBusProfile>())
                        profile.ConfigureMessages(cfg);

                    foreach (var profile in ctx.GetServices<IEventBusProfile>())
                        profile.ConfigureMessages(cfg);
                });
            });

            services.AddScoped<ICommandSender, MassTransitCommandSender>();

            return services;
        }

        public static IServiceCollection AddCommandBusProfiles(this IServiceCollection services)
        {
            services.AddTransient<ICommandBusProfile, OrderCreatedCommandBusProfile>();
            return services;
        }

        public static IServiceCollection AddEventBusProfiles(this IServiceCollection services)
        {
            services.AddTransient<IEventBusProfile, PriceTickPublishedEventBusProfile>();
            services.AddTransient<IEventBusProfile, NeedSymbolEventBusProfile>();
            services.AddTransient<IEventBusProfile, ReleaseSymbolEventBusProfile>();
            return services;
        }
    }
}
