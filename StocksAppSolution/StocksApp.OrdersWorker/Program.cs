using MassTransit;
using StocksApp.Core.IoC;
using StocksApp.Infrastructure.Helpers;
using StocksApp.Infrastructure.IoC;
using StocksApp.Observability;
using StocksApp.OrdersWorker.IoC;
using StocksApp.OrdersWorker.MessageBroker;
using StocksApp.OrdersWorker.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ServiceProviderOptions>(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddHostedService<OrdersWorker>();

builder.Services
    .AddWorkerServices()
    .AddPersistence(builder.Configuration, builder.Environment)
    .AddInfrastructureMessaging(
        builder.Configuration,
        registerConsumers: x =>
        {
            x.AddConsumer<SellOrderCreatedConsumer>();
        },
        configureReceiveEndpoints: (cfg, ctx) =>
        {
            cfg.ReceiveEndpoint(RabbitMQQueues.SellOrderCreatedQueue, e =>
            {
                e.Bind(RabbitMQExchanges.OrdersExchange, b =>
                {
                    b.ExchangeType = "direct";
                    b.RoutingKey = "sellorder.created";
                });

                e.ConfigureConsumer<SellOrderCreatedConsumer>(ctx);
            });
        })
    .AddPriceFeedSubscriber(builder.Configuration);

if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddObservability(builder.Configuration);
}

var host = builder.Build();

host.Run();
