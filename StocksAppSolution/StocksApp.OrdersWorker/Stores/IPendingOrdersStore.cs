using StocksApp.Domain.Events;

namespace StocksApp.OrdersWorker.Stores
{
    /// <summary>
    /// Holds pending orders in memory and answers queries about them. 
    /// </summary>
    public interface IPendingOrdersStore
    {
        IReadOnlyList<SellOrderCreatedCommand> TakeTriggeredOrders(string stockSymbol, double currentPrice);
        void AddSellOrder(SellOrderCreatedCommand sellOrderCreatedCommand);
        void RemoveSellOrder(SellOrderCreatedCommand sellOrderCreatedCommand);
    }
}
