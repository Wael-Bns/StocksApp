using StocksApp.Infrastructure.IoC;
using StocksApp.PriceFeed.IoC;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPriceFeedServices()
    .AddRabbitMqConsumers(builder.Configuration)
    .AddRabbitMqProducers(builder.Configuration);

var host = builder.Build();
host.Run();
