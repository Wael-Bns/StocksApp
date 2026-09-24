using StocksApp.Core;
using StocksApp.Infrastructure.IoC;
using StocksApp.PriceFeed.IoC;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPriceFeedServices()
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddInfrastructureMessaging(builder.Configuration);


var host = builder.Build();
host.Run();
