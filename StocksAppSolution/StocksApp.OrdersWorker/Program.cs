using StocksApp.OrdersWorker;
using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Services;
using StocksApp.Core;
using StocksApp.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IPriceUpdateOrderProcessor, PriceUpdateOrderProcessor>();
builder.Services.AddSingleton<IWorkerSubscriptionsManager, WorkerSubscriptionsManager>();
builder.Services.AddHostedService<OrdersWorker>();
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddCore(builder.Configuration);

var host = builder.Build();

host.Run();
