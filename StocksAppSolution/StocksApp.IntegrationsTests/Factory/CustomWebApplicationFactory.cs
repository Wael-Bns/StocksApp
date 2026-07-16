using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using StocksApp.Core.HttpClientAbstractions;
using StocksApp.Core.WebSocketClientAbstractions;
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

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ApplicationDbContext>();

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(_postgresDbManager.ConnectionString);
                });

                services.RemoveAll<IFinnHubHttpClient>();

                services.AddScoped<IFinnHubHttpClient, FakeFinnhubHttpClient>();

                services.RemoveAll<IFinnhubWebSocketClient>();
                services.AddSingleton<IFinnhubWebSocketClient, FakeFinnhubWebSocketClient>();

                var massTransitDescriptors = services
                    .Where(d => d.ServiceType.Namespace != null
                             && d.ServiceType.Namespace.Contains("MassTransit", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var descriptor in massTransitDescriptors)
                    services.Remove(descriptor);

                services.AddMassTransitTestHarness();
            });
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _postgresDbManager.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
