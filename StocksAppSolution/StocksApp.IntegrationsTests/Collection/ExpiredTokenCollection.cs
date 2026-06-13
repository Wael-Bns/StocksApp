using StocksApp.IntegrationsTests.Factory;

namespace StocksApp.IntegrationsTests.Collection
{
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class ExpiredTokenCollection : ICollectionFixture<ExpiredTokenWebApplicationFactory>
    {
        public const string Name = "ExpiredToken";
    }
}
