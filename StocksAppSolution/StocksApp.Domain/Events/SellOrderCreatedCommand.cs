namespace StocksApp.Domain.Events
{
    public class SellOrderCreatedCommand
    {
        public Guid SellOrderId { get; }
        public Guid UserId { get; }
        public double Price { get; }
        public int Quantity { get; }
    }
}
