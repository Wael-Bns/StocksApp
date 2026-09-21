using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using StocksApp.Infrastructure.Options;

namespace StocksApp.WebApi.IoC
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddInfrastructureHealthChecks(
            this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMqOptions = configuration
                .GetSection(RabbitMqOptions.SectionName)
                .Get<RabbitMqOptions>();

            var postgresConnectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is missing.");

            var healthChecksBuilder = services.AddHealthChecks()
                .AddNpgSql(
                    postgresConnectionString,
                    name: "PostgreSQL",
                    tags: new[] { "db", "data" });

            if (rabbitMqOptions != null)
            {
                // Health check resolves this async factory on-demand when /health is called
                healthChecksBuilder.AddRabbitMQ(
                    async sp =>
                    {
                        var factory = new ConnectionFactory { Uri = new Uri(rabbitMqOptions.ToAmqpUri()) };
                        return await factory.CreateConnectionAsync();
                    },
                    name: "RabbitMQ",
                    tags: new[] { "messaging", "rabbitmq" });
            }

            return services;
        }
    }
}