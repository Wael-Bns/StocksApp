using StocksApp.OutboxDispatcher;
using StocksApp.OutboxDispatcher.IoC;
using StocksApp.Infrastructure.IoC;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<OutboxDispatcherService>();

builder.Services
    .AddOutboxDispatcherServices()
    .AddInfrastructure(builder.Configuration, builder.Environment);

var host = builder.Build();
host.Run();
