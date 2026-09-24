namespace StocksApp.Domain.Events
{
    /// <summary>
    /// Published by the producer
    /// </summary>
    public interface IPriceTickPublished
    {
        string Symbol { get; }

        decimal Price { get; }

        decimal? Volume { get; }

        DateTimeOffset TradedAtUtc { get; }
    }
}
