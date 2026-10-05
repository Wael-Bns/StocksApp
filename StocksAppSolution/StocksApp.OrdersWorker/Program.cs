using StocksApp.Infrastructure.IoC;
using StocksApp.Observability;
using StocksApp.OrdersWorker.IoC;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ServiceProviderOptions>(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services
    .AddWorkerServices()
    .AddPersistence(builder.Configuration, builder.Environment)
    .AddInfrastructureMessaging(builder.Configuration)
    .AddPriceFeedSubscriber(builder.Configuration);

if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddObservability(builder.Configuration);
}

var host = builder.Build();

host.Run();
