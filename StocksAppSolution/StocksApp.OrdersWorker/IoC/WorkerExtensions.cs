using System.Net.WebSockets;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using StocksApp.OrdersWorker.Resilience;

namespace StocksApp.OrdersWorker.IoC
{
    public static class WorkerExtensions
    {
        public static IServiceCollection AddWorkerServices(this IServiceCollection services)
        {;
            services.AddResiliencePipeline(ResilienceOptions.PriceFeedSubscriptionPipeline, (builder, context) =>
            {
                var logger = context.ServiceProvider.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("PriceFeedSubscription");

                builder
                    .AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 3,
                        Delay = TimeSpan.FromMilliseconds(200),
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                        ShouldHandle = new PredicateBuilder()
                            .Handle<WebSocketException>()
                            .Handle<IOException>()
                            .Handle<TimeoutRejectedException>(),
                        OnRetry = args =>
                        {
                            logger.LogWarning(args.Outcome.Exception,
                                "Price feed subscription/unsubscription attempt {Attempt} failed; retrying in {Delay}",
                                args.AttemptNumber + 1, args.RetryDelay);
                            return default;
                        }
                    })
                    .AddTimeout(TimeSpan.FromSeconds(3));
            });

            return services;
        }
    }
}
