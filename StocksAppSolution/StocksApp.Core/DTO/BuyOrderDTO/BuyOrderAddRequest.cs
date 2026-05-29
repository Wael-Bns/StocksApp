using System.ComponentModel.DataAnnotations;
using StocksApp.Core.CustomValidationAttributes;
using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.DTO.BuyOrderDTO
{
    public class BuyOrderAddRequest
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
        [Required(ErrorMessage = "User is not recognized")]
        public Guid UserId { get; set; }

        public BuyOrder ToBuyOrder()
        {
            return new BuyOrder
            {
                BuyOrderID = Guid.NewGuid(),
                StockSymbol = StockSymbol,
                StockName = StockName,
                DateAndTimeOfOrder = DateAndTimeOfOrder,
                Price = Price,
                Quantity = Quantity,
                UserId = UserId
            };
        }
    }
}
