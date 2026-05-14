using System.Collections.Concurrent;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class SubscriptionsManager : ISubscriptionsManager
    {
        public event Func<string, Task>? OnAddedStockSymbol;
        public event Func<string, Task>? OnRemovedStockSymbol;
        
        private readonly ConcurrentDictionary<string, byte> _subscribedStockSymbols = new();

        public async Task AddStockSymbol(string stockSymbol)
        {
            if(_subscribedStockSymbols.TryAdd(stockSymbol, 0))
            {
                if (OnAddedStockSymbol != null)
                {
                    // Await all subscribers concurrently
                    var tasks = OnAddedStockSymbol.GetInvocationList()
                        .Cast<Func<string, Task>>()
                        .Select(handler => handler(stockSymbol));
                    
                    await Task.WhenAll(tasks);
                }
            }
        }

        public async Task RemoveStockSymbol(string stockSymbol)
        {
            if(_subscribedStockSymbols.TryRemove(stockSymbol, out _))
            {
                if (OnRemovedStockSymbol != null)
                {
                    // Await all subscribers concurrently
                    var tasks = OnRemovedStockSymbol.GetInvocationList()
                        .Cast<Func<string, Task>>()
                        .Select(handler => handler(stockSymbol));
                    
                    await Task.WhenAll(tasks);
                }
            }
        }
    }
}
