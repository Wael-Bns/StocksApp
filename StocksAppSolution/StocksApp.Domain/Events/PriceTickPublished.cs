namespace StocksApp.Domain.Events
{
    public sealed record PriceTickPublished(
        string Symbol,
        decimal Price,
        decimal? Volume,
        DateTimeOffset TradedAtUtc) : IPriceTickPublished;
}
