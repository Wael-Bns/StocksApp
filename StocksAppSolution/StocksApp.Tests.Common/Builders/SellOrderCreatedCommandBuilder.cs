using StocksApp.Domain.Events;

namespace StocksApp.Tests.Common.Builders
{
    public class SellOrderCreatedCommandBuilder
    {
        private Guid _sellOrderId = Guid.NewGuid();
        private Guid _userId = Guid.NewGuid();
        private string _stockSymbol = "MSFT";
        private double _price = 100;
        private uint _quantity = 10;
        private DateTime _createdAt = DateTime.UtcNow;

        public SellOrderCreatedCommandBuilder WithSellOrderId(Guid sellOrderId)
        {
            _sellOrderId = sellOrderId;
            return this;
        }

        public SellOrderCreatedCommandBuilder WithUserId(Guid userId)
        {
            _userId = userId;
            return this;
        }

        public SellOrderCreatedCommandBuilder WithStockSymbol(string stockSymbol)
        {
            _stockSymbol = stockSymbol;
            return this;
        }

        public SellOrderCreatedCommandBuilder WithPrice(double price)
        {
            _price = price;
            return this;
        }

        public SellOrderCreatedCommandBuilder WithQuantity(uint quantity)
        {
            _quantity = quantity;
            return this;
        }

        public SellOrderCreatedCommandBuilder WithCreatedAt(DateTime createdAt)
        {
            _createdAt = createdAt;
            return this;
        }

        public SellOrderCreatedCommand Build() => new()
        {
            SellOrderId = _sellOrderId,
            UserId = _userId,
            StockSymbol = _stockSymbol,
            Price = _price,
            Quantity = _quantity,
            CreatedAt = _createdAt
        };
    }
}
