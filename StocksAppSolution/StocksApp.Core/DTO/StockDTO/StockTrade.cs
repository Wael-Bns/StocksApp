namespace StocksApp.Core.DTO.StockDTO
{
    public class StockTrade
    {
        public string? StockSymbol { get; set; }
        public string? StockName { get; set; }
        public double PricePerShare { get; set; }
        public string? Logo { get; set; }
    }
}
