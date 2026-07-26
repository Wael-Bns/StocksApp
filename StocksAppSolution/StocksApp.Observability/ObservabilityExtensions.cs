using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging; // <-- Add this namespace
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace StocksApp.Observability
{
    public static class ObservabilityExtensions
    {
        public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>()
                          ?? throw new InvalidOperationException("Missing Observability config section");

            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName: options.ServiceName);

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddOpenTelemetry(otlpLogging =>
                {
                    otlpLogging.SetResourceBuilder(resourceBuilder);
                    otlpLogging.IncludeFormattedMessage = true;
                    otlpLogging.IncludeScopes = true;
                    otlpLogging.AddOtlpExporter(o => o.Endpoint = new Uri(options.OtlpEndpoint));
                });
            });

            services.AddOpenTelemetry()
                .WithTracing(tracing => tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("Npgsql")
                    .AddSource("MassTransit")
                    .AddOtlpExporter(o => o.Endpoint = new Uri(options.OtlpEndpoint)))
                .WithMetrics(metrics => metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("MassTransit")
                    .AddOtlpExporter(o => o.Endpoint = new Uri(options.OtlpEndpoint)));

            return services;
        }
    }
}