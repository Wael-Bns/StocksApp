using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksApp.Core.DTO
{
    public class StockTrade
    {
        public string? StockSymbol { get; set; }
        public string? StockName { get; set; }
        public double Price { get; set; }
        public uint Quantity { get; set; }
        public string? Icon { get; set; }
    }
}
