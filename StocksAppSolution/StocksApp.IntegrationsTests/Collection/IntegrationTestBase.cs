using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.IntegrationsTests.Factory;

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
            Client.Dispose();
            return Task.CompletedTask;
        }

    }
}
