using System.Text.Json.Serialization;

namespace StocksApp.WebApi.Models
{
    public class FinnhubTradeMessage
    {
        [JsonPropertyName("data")]
        public List<FinnhubTrade>? Data { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
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
    }
}
