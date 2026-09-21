namespace StocksApp.OrdersWorker.Resilience
{
    public static class ResilienceOptions
    {
        public static string PriceFeedSubscriptionPipeline { get; } = "PriceFeedSubscription";
    }
}
