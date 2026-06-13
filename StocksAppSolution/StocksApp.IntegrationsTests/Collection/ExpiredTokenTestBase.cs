using StocksApp.IntegrationsTests.Factory;

namespace StocksApp.IntegrationsTests.Collection
{
    [Collection(ExpiredTokenCollection.Name)]
    public abstract class ExpiredTokenTestBase : IntegrationTestBase
    {
        protected ExpiredTokenTestBase(ExpiredTokenWebApplicationFactory factory)
            : base(factory) { }
    }
}
