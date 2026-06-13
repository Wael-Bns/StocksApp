using StocksApp.Core.DTO.StockDTO;

namespace StocksApp.Core.HttpClientAbstractions
{
    public interface IFinnHubHttpClient
    {
        Task<StockQuoteDTO?> GetStockQuote(string stockSymbol);
        Task<CompanyProfileDTO?> GetCompanyProfile(string stockSymbol);
    }
}
