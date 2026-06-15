using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using StocksApp.Core.HttpClientAbstractions;
using StocksApp.Infrastructure;
using StocksApp.IntegrationsTests.Fakes;

namespace StocksApp.IntegrationsTests.Factory
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgresDbManager _postgresDbManager = new();
        public async Task InitializeAsync()
        {
            await _postgresDbManager.InitializeAsync();
            _ = Server;
            await _postgresDbManager.ApplyMigrationsAsync(Services);
        }
        public async Task ResetDatabaseAsync()
        {
            await _postgresDbManager.ResetAsync();
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            builder.ConfigureLogging(logging => logging.ClearProviders());

            

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ApplicationDbContext>();

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(_postgresDbManager.ConnectionString);
                });

                // 2. Replace Finnhub client
                services.RemoveAll<IFinnHubHttpClient>();

                services.AddScoped<IFinnHubHttpClient, FakeFinnhubHttpClient>();
            });
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _postgresDbManager.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
