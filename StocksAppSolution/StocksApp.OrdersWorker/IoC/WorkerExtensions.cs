using System.Net.WebSockets;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using StocksApp.OrdersWorker.Channels;
using StocksApp.OrdersWorker.MessageHandlers;
using StocksApp.OrdersWorker.Resilience;
using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Services;
using StocksApp.OrdersWorker.Stores;

namespace StocksApp.OrdersWorker.IoC
{
    public static class WorkerExtensions
    {
        public static IServiceCollection AddWorkerServices(this IServiceCollection services)
        {
            services.AddSingleton<IWorkerChannel, WorkerChannel>();
            services.AddSingleton<IPendingOrdersStore, PendingSellOrdersStore>();
            services.AddSingleton<IPendingSellOrdersBootstrapper, PendingSellOrdersBootstrapper>();
            services.AddSingleton<IWorkerMessageDispatcher, WorkerMessageDispatcher>();
            services.AddSingleton<IWorkerMessageHandler, PriceUpdateMessageHandler>();
            services.AddSingleton<IWorkerMessageHandler, SellOrderCreatedMessageHandler>();
            services.AddSingleton<IPriceFeedSubscriptionRegistry, PriceFeedSubscriptionRegistry>();

            services.AddScoped<IOrdersExecutionService, OrdersExecutionService>();

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
