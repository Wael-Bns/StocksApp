using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Services;
using StocksApp.Core;
using StocksApp.OrdersWorker.Worker;
using StocksApp.Infrastructure.IOC;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IPriceUpdateOrderProcessor, PriceUpdateOrderProcessor>();
builder.Services.AddSingleton<IWorkerSubscriptionsManager, WorkerSubscriptionsManager>();

builder.Services.AddHostedService<OrdersWorker>();

builder.Services
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddCore(builder.Configuration)
    .AddRabbitMqCommandSender(builder.Configuration);

var host = builder.Build();

host.Run();
