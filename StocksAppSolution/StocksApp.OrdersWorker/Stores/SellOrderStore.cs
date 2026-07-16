using System.Collections.Concurrent;
using StocksApp.Domain.Events;
using StocksApp.OrdersWorker.Comparers;

namespace StocksApp.OrdersWorker.Stores
{
    public class SellOrdersStore : ISellOrdersStore
    {
        private readonly ConcurrentDictionary<string, SortedSet<SellOrderCreatedCommand>> _pendingSellOrders;
        private readonly ILogger<SellOrdersStore> _logger;
        public SellOrdersStore(ILogger<SellOrdersStore> logger)
        {
            _pendingSellOrders = new();
            _logger = logger;
        }
        public void AddSellOrder(SellOrderCreatedCommand sellOrderCreatedCommand)
        {
            var sortedSet = _pendingSellOrders.GetOrAdd(
                sellOrderCreatedCommand.StockSymbol!,
                _ => new SortedSet<SellOrderCreatedCommand>(new SellOrderCreatedCommandComparer())
            );

            lock (sortedSet)
            {
                sortedSet.Add(sellOrderCreatedCommand);
                _logger.LogInformation("Added sell order {OrderId} for stock {StockSymbol} at price {Price}", sellOrderCreatedCommand.SellOrderId, sellOrderCreatedCommand.StockSymbol, sellOrderCreatedCommand.Price);
            }
        }
        public void RemoveSellOrder(SellOrderCreatedCommand sellOrderCreatedCommand)
        {
            if (_pendingSellOrders.TryGetValue(sellOrderCreatedCommand.StockSymbol!, out var sortedSet))
            {
                lock (sortedSet)
                {
                    sortedSet.Remove(sellOrderCreatedCommand);
                    _logger.LogInformation("Removed sell order {OrderId} for stock {StockSymbol} at price {Price}", sellOrderCreatedCommand.SellOrderId, sellOrderCreatedCommand.StockSymbol, sellOrderCreatedCommand.Price);
                }
            }
        }
        public IReadOnlyList<SellOrderCreatedCommand> DequeueEligibleOrders(string stockSymbol, double currentPrice)
        {
            var eligible = new List<SellOrderCreatedCommand>();
            if (!_pendingSellOrders.TryGetValue(stockSymbol, out var sortedSet))
                return eligible;

            lock (sortedSet)
            {
                while (sortedSet.Count > 0 && sortedSet.Min!.Price <= currentPrice)
                {
                    eligible.Add(sortedSet.Min);
                    sortedSet.Remove(sortedSet.Min);
                }
            }
            return eligible;
        }
    }
}