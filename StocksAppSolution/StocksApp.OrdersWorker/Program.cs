using StocksApp.Core;
using StocksApp.Infrastructure.IoC;
using StocksApp.OrdersWorker.IoC;
using StocksApp.OrdersWorker.Worker;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

try
{
    builder.RegisterSerilog();

    builder.Services.AddHostedService<OrdersWorker>();

    builder.Services
        .AddInfrastructure(builder.Configuration, builder.Environment)
        .AddCore(builder.Configuration)
        .AddWorkerServices()
        .AddRabbitMqConsumers(builder.Configuration);

    var host = builder.Build();

    Log.Information("StocksApp Orders Worker is starting up...");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occurred during Orders Worker startup.");
}
finally
{
    Log.CloseAndFlush();
}
