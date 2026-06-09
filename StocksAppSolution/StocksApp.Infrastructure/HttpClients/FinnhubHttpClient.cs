using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.HttpClientAbstractions;

namespace StocksApp.Infrastructure.Services
{
    public class FinnhubHttpClient : IFinnHubHttpClient
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public FinnhubHttpClient(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _apiKey = _configuration["FinnhubApiKey"]!;
        }

        public async Task<StockQuoteDTO?> GetStockQuote(string stockSymbol)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"quote?symbol={stockSymbol}&token={_apiKey}");
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                StockQuoteResponse? stockQuoteResponse = JsonConvert.DeserializeObject<StockQuoteResponse>(jsonResponse);
                
                if(stockQuoteResponse == null) { return null; }

                return new StockQuoteDTO { CurrentPrice = stockQuoteResponse.CurrentPrice };
            }
            return null;
        }

        public async Task<CompanyProfileDTO?> GetCompanyProfile(string stockSymbol)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"stock/profile2?symbol={stockSymbol}&token={_apiKey}");
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                
                CompanyProfileResponse? companyProfileResponse = JsonConvert.DeserializeObject<CompanyProfileResponse>(jsonResponse);
                
                if (companyProfileResponse == null) { return null; }
                return new CompanyProfileDTO
                {
                    Country = companyProfileResponse.Country,
                    Name = companyProfileResponse.Name,
                    Currency = companyProfileResponse.Currency,
                    Industry = companyProfileResponse.FinnhubIndustry,
                    Exchange = companyProfileResponse.Exchange,
                    Logo = companyProfileResponse.Logo,
                    Ticker = companyProfileResponse.Ticker,
                    WebUrl = companyProfileResponse.WebUrl,
                    FinnhubIndustry = companyProfileResponse.FinnhubIndustry
                };
            }
            return null;
        }

        private class StockQuoteResponse
        {
            [JsonProperty("c")]
            public decimal CurrentPrice { get; set; }

            [JsonProperty("h")]
            public decimal HighPrice { get; set; }

            [JsonProperty("l")]
            public decimal LowPrice { get; set; }

            [JsonProperty("o")]
            public decimal OpenPrice { get; set; }

            [JsonProperty("pc")]
            public decimal PreviousClosePrice { get; set; }

            [JsonProperty("t")]
            public long Timestamp { get; set; }
        }

        private class CompanyProfileResponse
        {
            [JsonProperty("country")]
            public string? Country { get; set; }

            [JsonProperty("currency")]
            public string? Currency { get; set; }

            [JsonProperty("exchange")]
            public string? Exchange { get; set; }

            [JsonProperty("ipo")]
            public string? Ipo { get; set; }

            [JsonProperty("marketCapitalization")]
            public decimal MarketCapitalization { get; set; }

            [JsonProperty("name")]
            public string? Name { get; set; }

            [JsonProperty("phone")]
            public string? Phone { get; set; }

            [JsonProperty("shareOutstanding")]
            public decimal ShareOutstanding { get; set; }

            [JsonProperty("ticker")]
            public string? Ticker { get; set; }

            [JsonProperty("weburl")]
            public string? WebUrl { get; set; }

            [JsonProperty("logo")]
            public string? Logo { get; set; }

            [JsonProperty("finnhubIndustry")]
            public string? FinnhubIndustry { get; set; }
        }
    }
}
