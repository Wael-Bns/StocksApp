using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.DTO
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
                TradeAmount = sellOrder.Quantity * sellOrder.Price
            };
        }
    }
}
