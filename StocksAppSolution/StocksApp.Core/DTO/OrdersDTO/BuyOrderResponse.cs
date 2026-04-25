using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.DTO.OrdersDTO
{
    public class BuyOrderResponse
    {
        public Guid BuyOrderID { get; set; }
        public string? StockSymbol { get; set; }
        public string? StockName { get; set; }
        public DateTime DateAndTimeOfOrder { get; set; }
        public uint Quantity { get; set; }
        public double Price { get; set; }
        public double TradeAmount { get; set; }
    }
    public static class BuyOrderResponseExtensions
    {
        /// <summary>
        /// A method to convert a BuyOrder entity to a BuyOrderResponse DTO, calculating the TradeAmount as Quantity multiplied by Price.
        /// </summary>
        /// <param name="buyOrder"></param>
        /// <returns>An object of the BuyOrder class type</returns>
        public static BuyOrderResponse ToBuyOrderResponse(this BuyOrder buyOrder)
        {
            return new BuyOrderResponse
            {
                BuyOrderID = buyOrder.BuyOrderID,
                StockSymbol = buyOrder.StockSymbol,
                StockName = buyOrder.StockName,
                DateAndTimeOfOrder = buyOrder.DateAndTimeOfOrder,
                Quantity = buyOrder.Quantity,
                Price = buyOrder.Price,
                TradeAmount = buyOrder.Quantity * buyOrder.Price
            };
        }
    }
}
