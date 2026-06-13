using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace StocksApp.IntegrationsTests.Factory
{
    public class ExpiredTokenWebApplicationFactory : CustomWebApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JWT:EXPIRATION_MINUTES"] = "-10"
                });
            });
        }
    }
}
