using System.Runtime.CompilerServices;
using StocksApp.Domain.Entities;

namespace StocksApp.Domain.Events
{
    public class SellOrderCreatedCommand
    {
        public Guid SellOrderId { get; set; }
        public Guid UserId { get; set; }
        public string StockSymbol { get; set; } = string.Empty;
        public double Price { get; set; }
        public uint Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public static class SellOrderCreatedCommandExtensions
    {
        public static SellOrderCreatedCommand ToSellOrderCreatedCommand(this SellOrder sellOrder)
        {
            return new SellOrderCreatedCommand
            {
               SellOrderId = sellOrder.SellOrderID,
                UserId = sellOrder.UserId,
                StockSymbol = sellOrder.StockSymbol ?? string.Empty,
                Price = sellOrder.Price,
                Quantity = sellOrder.Quantity,
                CreatedAt = sellOrder.DateAndTimeOfOrder
            };
        }
    }
}
