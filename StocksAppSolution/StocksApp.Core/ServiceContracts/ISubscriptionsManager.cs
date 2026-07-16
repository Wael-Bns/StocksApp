namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// Manages the stock symbols to which clients are subscribed. 
    /// </summary>
    public interface ISubscriptionsManager
    {
        event Func<string, Task>? OnAddedStockSymbol;
        event Func<string, Task>? OnRemovedStockSymbol;
        Task AddStockSymbol(string stockSymbol);
        Task RemoveStockSymbol(string stockSymbol);
    }
}
