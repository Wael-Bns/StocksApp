using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Services;
using StocksApp.Core;
using StocksApp.Infrastructure;
using StocksApp.OrdersWorker.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IPriceUpdateOrderProcessor, PriceUpdateOrderProcessor>();
builder.Services.AddSingleton<IWorkerSubscriptionsManager, WorkerSubscriptionsManager>();

builder.Services.AddHostedService<OrdersWorker>();

builder.Services
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddCore(builder.Configuration);

var host = builder.Build();

host.Run();
