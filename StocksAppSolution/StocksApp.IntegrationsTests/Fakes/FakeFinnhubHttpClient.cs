using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.HttpClientAbstractions;

namespace StocksApp.IntegrationsTests.Fakes
{
    public class FakeFinnhubHttpClient : IFinnHubHttpClient
    {
        public Task<CompanyProfileDTO?> GetCompanyProfile(string stockSymbol)
        {
            var result = new CompanyProfileDTO
            {
                Country = "US",
                Exchange = "NASDAQ NMS - GLOBAL MARKET",
                Industry = "Technology",
                Currency = "USD",
                Name = "Apple Inc",
                Ticker = stockSymbol,
                WebUrl = "https://www.apple.com/",
                Logo = "https://static2.finnhub.io/file/publicdatany/finnhubimage/stock_logo/AAPL.png",
                FinnhubIndustry = "Technology"
            };

            return Task.FromResult<CompanyProfileDTO?>(result);
        }

        public Task<StockQuoteDTO?> GetStockQuote(string stockSymbol)
        {
            var result = new StockQuoteDTO
            {
                CurrentPrice = 150.25m
            };

            return Task.FromResult<StockQuoteDTO?>(result);
        }
    }
}