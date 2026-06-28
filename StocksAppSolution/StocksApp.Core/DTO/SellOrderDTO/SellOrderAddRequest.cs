using System.ComponentModel.DataAnnotations;
using StocksApp.Core.CustomValidationAttributes;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;

namespace StocksApp.Core.DTO.SellOrderDTO
{
    public class SellOrderAddRequest
    {
        [Required(ErrorMessage = "Required Stock symbol")]
        public string? StockSymbol { get; set; }
        [Required(ErrorMessage = "Stock name is mandatory")]
        public string? StockName { get; set; }
        [MinDate("2000-01-01")]
        public DateTime DateAndTimeOfOrder { get; set; }
        [Range(1, 10000, ErrorMessage = "Quantity should be between 1 and 10000")]
        public uint Quantity { get; set; }
        [Range(1, 10000, ErrorMessage = "Price should be between 1 and 10000")]
        public double Price { get; set; }
        public SellOrder ToSellOrder()
        {
            return new SellOrder
            {
                SellOrderID = Guid.NewGuid(),
                StockName = StockName,
                StockSymbol = StockSymbol,
                DateAndTimeOfOrder = DateAndTimeOfOrder,
                Price = Price,
                Quantity = Quantity,
                Status = SellOrderStatus.Pending,
            };
        }
    }
}
