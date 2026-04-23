using System.Collections.Immutable;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.MessageQueueContracts;

namespace StocksApp.Infrastructure.MessageQueue
{
    public class SellOrdersStore : ISellOrdersStore
    {
        private Dictionary<string,SortedSet<SellOrder>>? _ordersBook;
        private readonly ImmutableList<string> _availableStockSymbols =
        [    "AAPL", "MSFT", "NVDA", "AMZN", "META", "GOOGL", "GOOG", "TSLA", "BRK.B", "AVGO",
            "LLY", "JPM", "V", "UNH", "XOM", "MA", "JNJ", "PG", "HD", "COST",
            "MRK", "CVX", "ABBV", "CRM", "AMD", "PEP", "BAC", "KO", "WMT", "TMO",
            "NFLX", "MCD", "WFC", "CSCO", "INTC", "ADBE", "QCOM", "TXN", "DIS", "BA",
            "VZ", "PFE", "NKE", "IBM", "ORCL", "CAT", "GS", "UBER", "NOW", "AMAT"
        ];
        public Task AddSellOrder(SellOrder sellOrder)
        {
            if (string.IsNullOrEmpty(sellOrder.StockSymbol) || !_availableStockSymbols.Contains(sellOrder.StockSymbol!))
            {
                throw new ArgumentException("Invalid stock symbol");
            }
            // Add the sell order in the set
            _ordersBook[sellOrder.StockSymbol!].Add(sellOrder);
            return Task.CompletedTask;
        }

        public Task<List<string>> GetAvailableStockSymbols()
        {
            return Task.FromResult(_availableStockSymbols.ToList());
        }

        public Task<List<SellOrder>> GetValidSellOrders(string stockSymbol, double validPrice)
        {
            List<SellOrder> validSellOrders = new List<SellOrder>();
            if (!string.IsNullOrEmpty(stockSymbol) || !_availableStockSymbols.Contains(stockSymbol!))
            {
                throw new ArgumentException("Invalid stock symbol");
            }
            while (_ordersBook[stockSymbol].Any() && _ordersBook[stockSymbol].Min.Price <= validPrice)
            {
                validSellOrders.Add(_ordersBook[stockSymbol].Min);
                _ordersBook[stockSymbol].Remove(_ordersBook[stockSymbol].Min);
            }
            return Task.FromResult(validSellOrders);
        }
    }
}
