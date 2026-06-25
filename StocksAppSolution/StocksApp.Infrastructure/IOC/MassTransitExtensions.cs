using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.MessageBroker.Publisher;
using StocksApp.Infrastructure.Helpers;
using StocksApp.Infrastructure.MessageBroker;
using StocksApp.Infrastructure.MessageBroker.Profiles;

namespace StocksApp.Infrastructure.IoC
{
    public static class MassTransitExtensions
    {
        public static IServiceCollection AddRabbitMqCommandSender(this IServiceCollection services,IConfiguration configuration)
        {
            var settings = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>()
                ?? throw new InvalidOperationException("RabbitMQ settings are not configured.");

            services.AddCommandBusProfiles();

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(settings.HostName, "/", h =>
                    {
                        h.Username(settings.UserName!);
                        h.Password(settings.Password!);
                    });

                    foreach (var profile in ctx.GetServices<ICommandBusProfile>())
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
    }
}
