using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Infrastructure.IoC;
using StocksApp.Infrastructure.Repositories;
using StocksApp.Observability;
using StocksApp.OrdersWorker.BackgroundServices;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ServiceProviderOptions>(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services
    .AddPersistence(builder.Configuration, builder.Environment)
    .Configure<OrderMatchingOptions>(builder.Configuration.GetSection(OrderMatchingOptions.SectionName));

builder.Services.AddScoped<ISellOrderMatchRepository, SellOrderMatchRepository>();
builder.Services.AddScoped<IOrderMatcher, OrderMatcher>();
builder.Services.AddHostedService<SellOrderMatchingService>();



if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddObservability(builder.Configuration);
}

var host = builder.Build();

host.Run();
