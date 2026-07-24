using StocksApp.Core;
using StocksApp.Infrastructure.IoC;
using StocksApp.OrdersWorker.IoC;
using StocksApp.OrdersWorker.Worker;
using StocksApp.Observability;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<OrdersWorker>();

builder.Services
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddCore(builder.Configuration)
    .AddWorkerServices()
    .AddRabbitMqConsumers(builder.Configuration)
    .AddObservability(builder.Configuration);

var host = builder.Build();

host.Run();
