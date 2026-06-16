using System.Net.Http.Json;
using StocksApp.Core.DTO.BuyOrderDTO;
using StocksApp.Core.DTO.SellOrderDTO;

namespace StocksApp.IntegrationsTests.Helpers
{
    public class TradeHelper
    {
        private readonly HttpClient _client;
        private const string TradeInfoRoute = "/api/trade/trade-info";
        private const string BuyOrderRoute = "/api/trade/buyorder";
        private const string SellOrderRoute = "/api/trade/sellorder";
        private const string AllBuyOrdersRoute = "/api/trade/allbuyorders";
        private const string AllSellOrdersRoute = "/api/trade/allsellorders";

        public TradeHelper(HttpClient client)
        {
            _client = client;
        }

        public async Task<HttpResponseMessage> GetTradeInfoRawAsync(string? stockSymbol = null)
        {
            var url = string.IsNullOrWhiteSpace(stockSymbol)
                ? TradeInfoRoute
                : $"{TradeInfoRoute}/{stockSymbol}";

            return await _client.GetAsync(url);
        }

        public async Task<HttpResponseMessage> BuyOrderRawAsync(BuyOrderAddRequest request)
            => await _client.PostAsJsonAsync(BuyOrderRoute, request);

        public async Task<HttpResponseMessage> SellOrderRawAsync(SellOrderAddRequest request)
            => await _client.PostAsJsonAsync(SellOrderRoute, request);

        public async Task<HttpResponseMessage> GetAllBuyOrdersRawAsync()
            => await _client.GetAsync(AllBuyOrdersRoute);

        public async Task<HttpResponseMessage> GetAllSellOrdersRawAsync()
            => await _client.GetAsync(AllSellOrdersRoute);
    }
}