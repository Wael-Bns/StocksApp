using Microsoft.Extensions.Options;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using StocksApp.Infrastructure.IoC;
using StocksApp.Infrastructure.LeaderElection;
using StocksApp.Infrastructure.Options;
using StocksApp.Infrastructure.Services;
using StocksApp.PriceFeed.IoC;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPriceFeedServices(builder.Configuration)
    .AddPersistence(builder.Configuration, builder.Environment)
    .AddFinnhubClient(builder.Configuration)
    .AddInfrastructureMessaging(builder.Configuration)
    .AddLeaderElection(builder.Configuration)
    .AddCandleCache(builder.Configuration);


builder.Services.AddSingleton<ITrackedSymbolStore, TrackedSymbolStore>();
builder.Services.AddSingleton<ITrackedSymbolsNotifier>(sp =>
    new PostgresTrackedSymbolsNotifier(
        sp.GetRequiredService<IOptions<LeaderElectionOptions>>().Value,
        sp.GetRequiredService<ILogger<PostgresTrackedSymbolsNotifier>>()));

builder.Services.AddSingleton<ISubscriptionReconciler, SubscriptionReconciler>();

var host = builder.Build();

if (host.Services.GetRequiredService<IOptions<CandleCacheOptions>>().Value.Enabled)
{
    var reconciler = host.Services.GetRequiredService<ISubscriptionReconciler>();
    var aggregator = host.Services.GetRequiredService<IOhlcBarAggregator>();
    reconciler.SymbolUnsubscribed += aggregator.FlushAndRemoveSymbolAsync;
}

host.Run();
