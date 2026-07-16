using StocksApp.Domain.Events;

namespace StocksApp.OrdersWorker.Stores
{
    /// <summary>
    /// Stores pending sell orders in memory. 
    /// </summary>
    public interface ISellOrdersStore
    {
        IReadOnlyList<SellOrderCreatedCommand> DequeueEligibleOrders(string stockSymbol, double currentPrice);
        void AddSellOrder(SellOrderCreatedCommand sellOrderCreatedCommand);
        void RemoveSellOrder(SellOrderCreatedCommand sellOrderCreatedCommand);
    }
}
