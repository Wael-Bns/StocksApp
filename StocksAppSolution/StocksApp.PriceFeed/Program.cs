using MassTransit;
using Microsoft.EntityFrameworkCore;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Infrastructure;
using StocksApp.Infrastructure.Helpers;
using StocksApp.Infrastructure.IoC;
using StocksApp.Infrastructure.Repositories;
using StocksApp.Infrastructure.Services;
using StocksApp.PriceFeed.Consumers;
using StocksApp.PriceFeed.IoC;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPriceFeedServices()
    .AddPersistence(builder.Configuration, builder.Environment)
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

builder.Services.AddSingleton<ITrackedSymbolStore, TrackedSymbolStore>();

var host = builder.Build();
host.Run();
