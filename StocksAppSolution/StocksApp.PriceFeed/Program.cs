using StocksApp.PriceFeed.IoC;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddPriceFeedServices();

var host = builder.Build();
host.Run();
