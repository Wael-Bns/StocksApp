using MassTransit;
using StocksApp.Infrastructure.Helpers;
using StocksApp.Infrastructure.IoC;
using StocksApp.PriceFeed.Consumers;
using StocksApp.PriceFeed.IoC;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPriceFeedServices()
    .AddFinnhubClient(builder.Configuration)
    .AddInfrastructureMessaging(
        builder.Configuration,
        registerConsumers: x =>
        {
            x.AddConsumer<NeedSymbolConsumer>();
            x.AddConsumer<ReleaseSymbolConsumer>();
        },
        configureReceiveEndpoints: (cfg, ctx) =>
        {
            cfg.ReceiveEndpoint(RabbitMQQueues.NeedSymbolQueue, e =>
                e.ConfigureConsumer<NeedSymbolConsumer>(ctx));

            cfg.ReceiveEndpoint(RabbitMQQueues.ReleaseSymbolQueue, e =>
                e.ConfigureConsumer<ReleaseSymbolConsumer>(ctx));
        });



var host = builder.Build();
host.Run();
