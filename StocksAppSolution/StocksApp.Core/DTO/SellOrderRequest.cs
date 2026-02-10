using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StocksApp.Core.CustomValidationAttributes;
using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.DTO
{
    public class SellOrderRequest
    {
        [Required(ErrorMessage = "Required Stock symbol")]
        public string? StockSymbol { get; set; }
        [Required(ErrorMessage = "Stock name is mandatory")]
        public string? StockName { get; set; }
        [MinDate("2000-01-01")]
        public DateTime DateAndTimeOfOrder { get; set; }
        [Range(1, 10000, ErrorMessage = "Quantity should be between 1 and 100000")]
        public uint Quantity { get; set; }
        [Range(1, 10000, ErrorMessage = "Quantity should be between 1 and 100000")]
        public double Price { get; set; }
        
        public SellOrder ToSellOrder()
        {
            return new SellOrder
            {
                SellOrderID = Guid.NewGuid(),
                StockName = StockName,
                StockSymbol = StockSymbol,
                Price = Price,
                Quantity = Quantity
            };
        }
    }
}
