using System;
using System.ComponentModel.DataAnnotations;
using StocksApp.Core.CustomValidationAttributes;
using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.DTO
{
    public class BuyOrderRequest
    {
        [Required(ErrorMessage = "Required Stock symbol")]
        public string? StockSymbol { get; set; }
        [Required(ErrorMessage = "Stock name is mandatory")]
        public string? StockName { get; set; }
        [MinDate("2000-01-01")]
        public DateTime DateAndTimeOfOrder { get; set; }
        [Range(1, 10000,ErrorMessage ="Quantity should be between 1 and 100000")]
        public uint Quantity { get; set; }
        [Range(1, 10000, ErrorMessage = "Quantity should be between 1 and 100000")]
        public double Price { get; set; }

        public BuyOrder ToBuyOrder()
        {
            return new BuyOrder
            {
                BuyOrderID = Guid.NewGuid(),
                StockSymbol = this.StockSymbol,
                StockName = this.StockName,
                DateAndTimeOfOrder = this.DateAndTimeOfOrder,
                Price = this.Price,
                Quantity = this.Quantity
            };
        }
    }
}
