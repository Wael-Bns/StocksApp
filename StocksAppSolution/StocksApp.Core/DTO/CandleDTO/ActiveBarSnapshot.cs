namespace StocksApp.Core.DTO.CandleDTO
{
    public sealed record ActiveBarSnapshot(
        string Symbol,
        DateTimeOffset BucketStart,
        decimal Open,
        decimal High,
        decimal Low,
        decimal Close,
        long Volume,
        int TradeCount);
}