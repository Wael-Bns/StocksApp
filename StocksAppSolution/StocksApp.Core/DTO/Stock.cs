using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksApp.Core.DTO
{
    /// <summary>
    /// A class for returning search data of stocks from FinnHub API. It contains only stock symbol and stock name.
    /// </summary>
    public class Stock
    {
        public string? StockSymbol { get; set; }
        public string? StockName { get; set; }
    }
}
