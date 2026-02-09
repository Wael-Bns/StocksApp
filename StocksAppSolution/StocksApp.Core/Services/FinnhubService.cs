using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        private async Task<Dictionary<string,object>?> GetData(string url)
        {
            var apiKey = _configuration["FinnhubApiKey"];
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to fetch company profile. Status code: {response.StatusCode}");
            }
            try
            {
                string json = await response.Content.ReadAsStringAsync();
                Dictionary<string, object>? companyProfile = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
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
            string url = $"https://finnhub.io/api/v1/stock/profile2?symbol={stockSymbol}&token={_configuration["FinnhubApiKey"]}";

            var companyData = await GetData(url);

            return companyData;
        }

        public async Task<Dictionary<string, object>?> GetStockPriceQuote(string stockSymbol)
        {
            if (string.IsNullOrEmpty(stockSymbol))
            {
                throw new ArgumentException("Stock symbol cannot be empty or null", nameof(stockSymbol));
            }
            string url = $"https://finnhub.io/api/v1/quote?symbol={stockSymbol}&token={_configuration["FinnhubApiKey"]}";

            var companyData = await GetData(url);
            return companyData;
        }
    }
}
