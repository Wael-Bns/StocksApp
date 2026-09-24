namespace StocksApp.Infrastructure.Helpers
{
    public static class RabbitMQQueues
    {
        public const string SellOrderCreatedQueue = "sellorder.created.queue";
        public const string NeedSymbolQueue = "pricefeed.need-symbol";
        public const string ReleaseSymbolQueue = "pricefeed.release-symbol";
    }
}
