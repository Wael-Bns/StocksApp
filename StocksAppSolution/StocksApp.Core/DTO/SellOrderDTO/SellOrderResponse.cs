using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;
using StocksApp.Domain.Events;

namespace StocksApp.Core.DTO.SellOrderDTO
{
    public class SellOrderResponse
    {
        public Guid SellOrderID { get; set; }
        public string? StockSymbol { get; set; }
        public string? StockName { get; set; }
        public DateTime DateAndTimeOfOrder { get; set; }
        public uint Quantity { get; set; }
        public double Price { get; set; }
        public double TradeAmount { get; set; }
        public SellOrderStatus Status { get; set; }
    }
    public static class SellOrderResponseExtension
    {
        public static SellOrderResponse ToSellOrderResponse(this SellOrder sellOrder)
        {
            return new SellOrderResponse
            {
                SellOrderID = sellOrder.SellOrderID,
                StockName = sellOrder.StockName,
                StockSymbol = sellOrder.StockSymbol,
                DateAndTimeOfOrder = sellOrder.DateAndTimeOfOrder,
                Price = sellOrder.Price,
                Status = (SellOrderStatus)sellOrder.Status,
                Quantity = sellOrder.Quantity,
                TradeAmount = sellOrder.Quantity * sellOrder.Price
            };
        }
        public static SellOrderCreatedCommand ToSellOrderCreatedCommand(this SellOrderResponse sellOrderResponse, Guid userId)
        {
            return new SellOrderCreatedCommand
            {
                SellOrderId = sellOrderResponse.SellOrderID,
                UserId = userId, // Assuming you have a way to get the UserId, replace Guid.Empty with the actual UserId
                StockSymbol = sellOrderResponse.StockSymbol ?? string.Empty,
                Price = sellOrderResponse.Price,
                Quantity = sellOrderResponse.Quantity,
                CreatedAt = sellOrderResponse.DateAndTimeOfOrder
            };
        }
    }
}
