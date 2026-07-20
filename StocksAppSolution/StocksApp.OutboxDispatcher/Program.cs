using StocksApp.OutboxDispatcher;
using StocksApp.OutboxDispatcher.IoC;
using StocksApp.Infrastructure.IoC;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<OutboxDispatcherService>();

builder.Services
    .AddOutboxDispatcherServices(builder.Configuration)
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddRabbitMqCommandSender(builder.Configuration);

var host = builder.Build();
host.Run();
