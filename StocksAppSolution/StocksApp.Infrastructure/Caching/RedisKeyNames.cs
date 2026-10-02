namespace StocksApp.Infrastructure.Caching
{
    public static class RedisKeyNames
    {
        private const string LatestPricePrefix = "price:latest:";
        private const string ActiveBarPrefix = "bar:active:";

        public static string LatestPrice(string symbol) => LatestPricePrefix + symbol;
        public static string ActiveBar(string symbol) => ActiveBarPrefix + symbol;
        public static string LatestPriceScanPattern => LatestPricePrefix + "*";
        public static string ActiveBarScanPattern => ActiveBarPrefix + "*";

        public static string SymbolFromActiveBarKey(string key) => key[ActiveBarPrefix.Length..];
    }
}