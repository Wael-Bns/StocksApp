namespace StocksApp.Domain.Events
{
    public sealed record PriceTickPublished(
        string StockSymbol,
        double Price,
        double? Volume,
        DateTimeOffset TradedAtUtc) : IPriceTickPublished;
}
