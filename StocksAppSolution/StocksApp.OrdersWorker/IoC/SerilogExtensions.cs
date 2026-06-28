using Microsoft.AspNetCore.Builder;
using Serilog;

namespace StocksApp.OrdersWorker.IoC
{
    public static class SerilogExtensions
    {
        public static void RegisterSerilog(this HostApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services));
        }
    }
}
