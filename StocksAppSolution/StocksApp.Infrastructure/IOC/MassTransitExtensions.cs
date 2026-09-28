using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.MessageBroker.Publisher;
using StocksApp.Infrastructure.MessageBroker;
using StocksApp.Infrastructure.MessageBroker.Profiles;
using StocksApp.Infrastructure.Options;

namespace StocksApp.Infrastructure.IoC
{
    public static class MassTransitExtensions
    {
        public static IServiceCollection AddInfrastructureMessaging(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<IBusRegistrationConfigurator>? registerConsumers = null,
            Action<IRabbitMqBusFactoryConfigurator, IBusRegistrationContext>? configureReceiveEndpoints = null)
        {
            var rabbitMqSection = configuration.GetSection(RabbitMqOptions.SectionName);
            var settings = rabbitMqSection.Get<RabbitMqOptions>()
                ?? throw new InvalidOperationException("RabbitMQ settings are not configured.");

            services.Configure<RabbitMqOptions>(rabbitMqSection);
            services.AddCommandBusProfiles();
            services.AddEventBusProfiles();

            services.AddMassTransit(x =>
            {
                registerConsumers?.Invoke(x);

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(settings.HostName, "/", h =>
                    {
                        h.Username(settings.UserName!);
                        h.Password(settings.Password!);
                    });

                    configureReceiveEndpoints?.Invoke(cfg, ctx);

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
