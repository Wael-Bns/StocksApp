using StocksApp.OutboxDispatcher.IoC;
using StocksApp.Infrastructure.IoC;
using StocksApp.OutboxDispatcher.BackgroundServices;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<OutboxDispatcherService>();
builder.Services.AddHostedService<OutboxPollingBackgroundService>();

builder.Services
    .AddOutboxDispatcherServices(builder.Configuration)
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddInfrastructureMessaging(builder.Configuration);

var host = builder.Build();
host.Run();
