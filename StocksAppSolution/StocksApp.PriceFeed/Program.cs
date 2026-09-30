using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Infrastructure;
using StocksApp.Infrastructure.Helpers;
using StocksApp.Infrastructure.IoC;
using StocksApp.Infrastructure.LeaderElection;
using StocksApp.Infrastructure.Options;
using StocksApp.Infrastructure.Repositories;
using StocksApp.Infrastructure.Services;
using StocksApp.PriceFeed.Consumers;
using StocksApp.PriceFeed.IoC;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPriceFeedServices(builder.Configuration)
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
builder.Services.AddSingleton<ITrackedSymbolsNotifier>(sp =>
    new PostgresTrackedSymbolsNotifier(
        sp.GetRequiredService<IOptions<LeaderElectionOptions>>().Value,
        sp.GetRequiredService<ILogger<PostgresTrackedSymbolsNotifier>>()));

builder.Services.AddSingleton<ISubscriptionReconciler, SubscriptionReconciler>();

var host = builder.Build();
host.Run();
