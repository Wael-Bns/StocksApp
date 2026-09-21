using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            var options = configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>();

            // If configuration section is missing, default gracefully without crashing
            if (options == null)
            {
                return services;
            }

            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName: options.ServiceName ?? "UnknownService");

            bool hasEndpoint = !string.IsNullOrWhiteSpace(options.OtlpEndpoint);

            // Configure Logging
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddOpenTelemetry(otlpLogging =>
                {
                    otlpLogging.SetResourceBuilder(resourceBuilder);
                    otlpLogging.IncludeFormattedMessage = true;
                    otlpLogging.IncludeScopes = true;

                    if (hasEndpoint)
                    {
                        otlpLogging.AddOtlpExporter(o => o.Endpoint = new Uri(options.OtlpEndpoint!));
                    }
                });
            });

            // Configure Traces & Metrics
            services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    tracing.SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSource("Npgsql")
                        .AddSource("MassTransit");

                    if (hasEndpoint)
                    {
                        tracing.AddOtlpExporter(o => o.Endpoint = new Uri(options.OtlpEndpoint!));
                    }
                })
                .WithMetrics(metrics =>
                {
                    metrics.SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddMeter("MassTransit");

                    if (hasEndpoint)
                    {
                        metrics.AddOtlpExporter(o => o.Endpoint = new Uri(options.OtlpEndpoint!));
                    }
                });

            return services;
        }
    }
}