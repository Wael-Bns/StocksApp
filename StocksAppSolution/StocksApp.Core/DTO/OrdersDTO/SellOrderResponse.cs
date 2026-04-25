using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Enums;

namespace StocksApp.Core.DTO.OrdersDTO
{
    public class SellOrderResponse : IComparable<SellOrderResponse>
    {
        public Guid SellOrderID { get; set; }
        public string? StockSymbol { get; set; }
        public string? StockName { get; set; }
        public DateTime DateAndTimeOfOrder { get; set; }
        public uint Quantity { get; set; }
        public double Price { get; set; }
        public double TradeAmount { get; set; }
        public string SellOrderStatus { get; set; } = SellOrderStatusEnum.Pending.ToString();

        public int CompareTo(SellOrderResponse? other)
        {
            if(other == null)
            {
                return 1;
            }
            if(Price == other.Price)
            {
                return DateAndTimeOfOrder < other.DateAndTimeOfOrder ? 1 : -1;
            }
            return Price < other.Price ? 1 : -1;
        }
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
                Quantity = sellOrder.Quantity,
                TradeAmount = sellOrder.Quantity * sellOrder.Price,
                SellOrderStatus = sellOrder.SellOrderStatus
            };
        }
    }
}
