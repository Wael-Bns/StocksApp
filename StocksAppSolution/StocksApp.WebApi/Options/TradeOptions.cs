
namespace StocksApp.WebApi.Options
{
    public class TradeOptions
    {
        public const string SectionName = "TradingOptions";
        public string DefaultStockSymbol { get; set; } = "MSFT";
        public uint DefaultOrderQuantity { get; set; }
    }
}
