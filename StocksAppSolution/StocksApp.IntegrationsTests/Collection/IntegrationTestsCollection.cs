using StocksApp.IntegrationsTests.Factory;

namespace StocksApp.IntegrationsTests.Collection
{
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class IntegrationTestsCollection : ICollectionFixture<CustomWebApplicationFactory>
    {
        public const string Name = "IntegrationTestsCollection";
    }
}
