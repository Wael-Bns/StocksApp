namespace StocksApp.Core.DTO.StockDTO
{
    public class PriceUpdateMessage
    {
        public string StockSymbol { get; set; } = string.Empty;
        public double Price { get; set; }
        public long Timestamp { get; set; }
        public double Volume { get; set; }
    }
}
