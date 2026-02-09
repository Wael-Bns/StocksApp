using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class FinnhubService : IFinnHubService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public FinnhubService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        private async Task<Dictionary<string,object>> GetData(string type,string stockSymbol)
        {
            var apiKey = _configuration["FinnhubApiKey"];
            var requestUrl = $"https://finnhub.io/api/v1/stock/{type}?symbol={stockSymbol}&token={apiKey}";
            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to fetch company profile for {stockSymbol}. Status code: {response.StatusCode}");
            }
            string json = await response.Content.ReadAsStringAsync();
            Dictionary<string, object>? companyProfile = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (companyProfile == null)
            {
                throw new Exception($"Failed to deserialize company profile for {stockSymbol}");
            }

            return companyProfile;

        }
        public async Task<Dictionary<string, object>> GetCompanyProfile(string stockSymbol)
        {

            if(string.IsNullOrEmpty(stockSymbol))
            {
                throw new ArgumentException("Stock symbol cannot be empty or null", nameof(stockSymbol));
            }

            var companyData = await GetData("profile2", stockSymbol);
            return companyData;
        }

        public async Task<Dictionary<string, object>> GetStockPriceQuote(string stockSymbol)
        {
            if (string.IsNullOrEmpty(stockSymbol))
            {
                throw new ArgumentException("Stock symbol cannot be empty or null", nameof(stockSymbol));
            }

            var companyData = await GetData("quote", stockSymbol);
            return companyData;
        }
    }
}
