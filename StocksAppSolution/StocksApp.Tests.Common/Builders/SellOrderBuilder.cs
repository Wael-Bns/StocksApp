using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;

namespace StocksApp.Tests.Common.Builders
{
    public class SellOrderBuilder
    {
        private Guid _sellOrderId = Guid.NewGuid();
        private string _stockSymbol = "MSFT";
        private string _stockName = "Microsoft Corporation";
        private DateTime _dateAndTimeOfOrder = DateTime.UtcNow;
        private uint _quantity = 10;
        private double _price = 100;
        private SellOrderStatus _status = SellOrderStatus.Pending;
        private User? _user;
        private Guid? _userId;

        public SellOrderBuilder WithSellOrderId(Guid sellOrderId)
        {
            _sellOrderId = sellOrderId;
            return this;
        }

        public SellOrderBuilder WithStockSymbol(string stockSymbol)
        {
            _stockSymbol = stockSymbol;
            return this;
        }

        public SellOrderBuilder WithStockName(string stockName)
        {
            _stockName = stockName;
            return this;
        }

        public SellOrderBuilder WithDateAndTimeOfOrder(DateTime dateAndTimeOfOrder)
        {
            _dateAndTimeOfOrder = dateAndTimeOfOrder;
            return this;
        }

        public SellOrderBuilder WithQuantity(uint quantity)
        {
            _quantity = quantity;
            return this;
        }

        public SellOrderBuilder WithPrice(double price)
        {
            _price = price;
            return this;
        }

        public SellOrderBuilder WithStatus(SellOrderStatus status)
        {
            _status = status;
            return this;
        }

        public SellOrderBuilder WithUser(User user)
        {
            _user = user;
            _userId = user.UserId;
            return this;
        }

        public SellOrderBuilder WithUserId(Guid userId)
        {
            _userId = userId;
            return this;
        }

        public SellOrder Build()
        {
            var user = _user ?? new UserBuilder().WithUserId(_userId ?? Guid.NewGuid()).Build();

            return new SellOrder
            {
                SellOrderID = _sellOrderId,
                StockSymbol = _stockSymbol,
                StockName = _stockName,
                DateAndTimeOfOrder = _dateAndTimeOfOrder,
                Quantity = _quantity,
                Price = _price,
                Status = _status,
                UserId = _userId ?? user.UserId,
                User = user
            };
        }
    }
}
