using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.ServiceContracts;
using StocksApp.IntegrationsTests.Factory;
using StocksApp.Tests.Common.Fakes;

namespace StocksApp.IntegrationsTests.Collection
{
    [Collection(IntegrationTestsCollection.Name)]
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        protected readonly CustomWebApplicationFactory Factory;
        protected HttpClient Client { get; private set; }
        protected ITestHarness Harness { get; private set; }

        protected IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            Factory = factory;
            Client = Factory.CreateClient();
            Harness = Factory.Services.GetRequiredService<ITestHarness>();

        }
        public async Task InitializeAsync()
        {
            await Factory.ResetDatabaseAsync();
        }
        public Task DisposeAsync()
        {
            ((FakeCandleCache)Factory.Services.GetRequiredService<ICandleCache>()).Clear();
            Client.Dispose();
            return Task.CompletedTask;
        }

    }
}
