using StocksApp.Infrastructure.Options;
using RabbitMQ.Client;

namespace StocksApp.WebApi.IoC
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddInfrastructureHealthChecks(
            this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMqOptions = configuration
                .GetSection(RabbitMqOptions.SectionName)
                .Get<RabbitMqOptions>()!;

            services.AddSingleton<IConnection>(_ =>
            {
                var factory = new ConnectionFactory { Uri = new Uri(rabbitMqOptions.ToAmqpUri()) };
                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });

            services.AddHealthChecks()
                .AddNpgSql(
                    configuration.GetConnectionString("DefaultConnection")!,
                    name: "PostgreSQL")
                .AddRabbitMQ(name: "RabbitMQ");

            return services;
        }
    }
}