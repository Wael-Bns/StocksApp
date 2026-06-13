namespace StocksApp.Core.Domain.Entities
{
    public class SellOrder
    {
        public Guid SellOrderID { get; set; }
        public string? StockSymbol { get; set; }
        public string? StockName { get; set; }
        public DateTime DateAndTimeOfOrder { get; set; }
        public uint Quantity { get; set; }
        public double Price { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
