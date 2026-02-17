using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StocksApp.Core.DTO;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class FinnhubService : IFinnHubService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public FinnhubService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _apiKey = _configuration["FinnhubApiKey"];
        }
        private string BuildUrl(string endpoint, string stockSymbol)
        {
            return $"https://finnhub.io/api/v1/{endpoint}?symbol={stockSymbol}&token={_apiKey}";
        }
        private async Task<T> GetData<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to fetch company profile.\n Status code: {response.StatusCode}");
            }
            try
            {
                string json = await response.Content.ReadAsStringAsync();
                var companyProfile = JsonConvert.DeserializeObject<T>(json);
                return companyProfile;
            }
            catch(Exception ex)
            {
                throw new Exception($"Error deserializing company profile: {ex.Message}");
            }
        }
        public async Task<Dictionary<string, object>?> GetCompanyProfile(string stockSymbol) 
        {
            if(string.IsNullOrEmpty(stockSymbol))
            {
                throw new ArgumentException("Stock symbol cannot be empty or null", nameof(stockSymbol));
            }
            
            string url = BuildUrl("stock/profile2", stockSymbol);

            var companyData = await GetData<Dictionary<string, object>>(url);

            return companyData;
        }

        public async Task<Dictionary<string, object>?> GetStockPriceQuote(string stockSymbol)
        {
            if (string.IsNullOrEmpty(stockSymbol))
            {
                throw new ArgumentException("Stock symbol cannot be empty or null", nameof(stockSymbol));
            }
            string url = BuildUrl("quote", stockSymbol);

            var companyData = await GetData<Dictionary<string,object>>(url);
            return companyData;
        }

        public async Task<List<Stock>> SearchStocks(string query)
        {
            string url = $"https://finnhub.io/api/v1/search?q={query}&exchange=US&token={_configuration["FinnhubApiKey"]}";
            Dictionary<string, object> searchResults = await GetData<Dictionary<string, object>>(url);
            List<Dictionary<string, object>> results = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(searchResults["result"].ToString());

            List<Stock> stocks = [];
            for(int i=0;i<results.Count;i++)
            {
                stocks.Add(new Stock
                {
                    StockSymbol = results[i]["symbol"] as string,
                    StockName = results[i]["description"] as string
                });
            }

            return stocks;
        }
    }
}
