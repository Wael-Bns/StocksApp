using StocksApp.Domain.Constants;
using StocksApp.Domain.Entities;

namespace StocksApp.Domain.Events
{
    public class SellOrderExecuted : IOutboxEvent
    {
        public static string EventName => EventNames.SellOrderExecuted;

        public Guid SellOrderId { get; set; }
        public Guid UserId { get; set; }
        public string StockSymbol { get; set; } = string.Empty;
        public double Price { get; set; }
        public uint Quantity { get; set; }
        public DateTime ExecutedAt { get; set; }

        public static SellOrderExecuted From(SellOrder order) => new()
        {
            SellOrderId = order.SellOrderID,
            UserId = order.UserId,
            StockSymbol = order.StockSymbol,
            Price = order.Price,
            Quantity = order.Quantity,
            ExecutedAt = DateTime.UtcNow
        };
    }
}