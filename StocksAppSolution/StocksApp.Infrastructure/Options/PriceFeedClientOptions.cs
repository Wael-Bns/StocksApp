namespace StocksApp.Infrastructure.Options
{
    public class PriceFeedClientOptions
    {
        public const string SectionName = "PriceFeedClient";
        public string SubscriberId { get; set; } = Environment.MachineName;
    }
}
