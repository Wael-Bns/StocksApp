namespace StocksApp.Infrastructure.Helpers
{
    public static class RabbitMQExchanges
    {
        public const string OrdersExchange = "orders_exchange";
        public const string PricesExchange = "prices_exchange";
        public const string SymbolSubscriptionsExchange = "symbol_subscriptions_exchange";
        public const string NeedSymbolExchange = "need_symbol_exchange";
        public const string ReleaseSymbolExchange = "release_symbol_exchange";
    }
}
