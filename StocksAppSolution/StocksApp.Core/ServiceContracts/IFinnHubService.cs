using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}
