using System.ComponentModel.DataAnnotations;

namespace StocksApp.Core.DTO.SellOrderDTO
{
    public class SellOrderAddRequest
    {
        [Required(ErrorMessage = "Required Stock symbol")]
        public string? StockSymbol { get; set; }
        [Required(ErrorMessage = "Stock name is mandatory")]
        public string? StockName { get; set; }
        [Range(1, 10000, ErrorMessage = "Quantity should be between 1 and 10000")]
        public uint Quantity { get; set; }
        [Range(1, 10000, ErrorMessage = "Price should be between 1 and 10000")]
        public double Price { get; set; }
    }
}