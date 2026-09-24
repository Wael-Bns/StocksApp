namespace StocksApp.Domain.Events
{
    /// <summary>
    /// Published by the producer
    /// </summary>
    public interface IPriceTickPublished
    {
        string StockSymbol { get; }

        double Price { get; }

        double? Volume { get; }

        DateTimeOffset TradedAtUtc { get; }
    }
}
