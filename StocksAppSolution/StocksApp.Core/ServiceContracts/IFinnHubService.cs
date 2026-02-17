
using StocksApp.Core.DTO;

namespace StocksApp.Core.ServiceContracts
{
    public interface IFinnHubService
    {
        /// <summary>
        /// Get general information of a company based on the stock symbol.
        /// </summary>
        /// <param name="stockSymbol">Stock symbol of the desired company</param>
        /// <returns>General company information as key-value pairs</returns>
        Task<Dictionary<string, object>?> GetCompanyProfile(string stockSymbol);
        /// <summary>
        /// Get real-time quote data for US stocks (infos at the time of the request)
        /// </summary>
        /// <param name="stockSymbol">Stock symbol of the desired company</param>
        /// <returns>Quote data as key-value pairs</returns>
        Task<Dictionary<string, object>?> GetStockPriceQuote(string stockSymbol);
        /// <summary>
        /// Search for best-matching symbols based on your query.
        /// </summary>
        /// <param name="query">Query text can be symbol, name, isin, or cusip.</param>
        /// <returns></returns>
        Task<List<Stock>> SearchStocks(string query); 
    }
}
