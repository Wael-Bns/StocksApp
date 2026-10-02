namespace StocksApp.Core.DTO.CandleDTO
{
    public sealed record LatestPriceSnapshot(
        string Symbol,
        decimal Price,
        long Volume,
        long Timestamp);
}