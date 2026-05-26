using System.Text.Json.Serialization;
using StocksApp.Core.DTO.StockDTO;

namespace StocksApp.Infrastructure.Models
{
    public class FinnhubTradeMessage
    {
        [JsonPropertyName("data")]
        public List<FinnhubTrade>? Data { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
        public IReadOnlyCollection<PriceUpdateMessage>? ToPriceUpdateMessageList()
        {
            if(Data == null || Data.Count == 0 || Type != "trade")
            {
                return null;
            }
            return Data.Select(trade => trade.ToPriceUpdateMessage()).ToList().AsReadOnly();
        }
    }

    public class FinnhubTrade
    {
        // price
        [JsonPropertyName("p")]
        public decimal Price { get; set; }

        // symbol
        [JsonPropertyName("s")]
        public string Symbol { get; set; } = string.Empty;

        // timestamp
        [JsonPropertyName("t")]
        public long Timestamp { get; set; }

        // volume
        [JsonPropertyName("v")]
        public decimal Volume { get; set; }

        // conditions
        [JsonPropertyName("c")]
        public List<string> Conditions { get; set; } = new();
        public PriceUpdateMessage ToPriceUpdateMessage()
        {
            return new PriceUpdateMessage
            {
                StockSymbol = Symbol,
                Price = (double)Price,
                timestamp = Timestamp,
                Volume = (double)Volume
            };
        }
    }
    
}
