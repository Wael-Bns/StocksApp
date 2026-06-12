using StocksApp.IntegrationsTests.Factory;

namespace StocksApp.IntegrationsTests.Collection
{
    [Collection(IntegrationTestsCollection.Name)]
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        protected readonly CustomWebApplicationFactory Factory;
        protected HttpClient Client { get; private set; } = default!;
        protected IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            Factory = factory;
            Client = Factory.CreateClient();
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
