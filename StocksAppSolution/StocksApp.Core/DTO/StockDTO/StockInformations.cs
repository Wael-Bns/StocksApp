namespace StocksApp.Core.DTO.StockDTO
{
    /// <summary>
    /// A class for returning general information of a stock from third party API.
    /// </summary>
    public class StockInformations
    {
        public string? StockSymbol { get; set; }
        public string? StockName { get; set; }
        public string? Currency { get; set; }
        public string? Exchange { get; set; }
        public string? WebUrl { get; set; }
        public string? Industry { get; set; }
        public decimal PricePerShare { get; set; }
        public string? Logo { get; set; }
    }
}
