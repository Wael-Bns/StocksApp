namespace StocksApp.Infrastructure.Options
{
    public class FinnhubOptions
    {
        public const string ConfigurationName = "FinnhubApiKey";
        public string ApiKey { get; set; } = string.Empty;
    }
}
