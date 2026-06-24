namespace StocksApp.Core.DTO.Commands
{
    public class ExecuteSellOrderCommand
    {
        public Guid SellOrderId { get; }
        public Guid UserId { get; }
        public double Price { get; }
        public int Quantity { get; }
    }
}
