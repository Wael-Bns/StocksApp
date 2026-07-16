using StocksApp.Core.DTO.SellOrderDTO;

namespace StocksApp.Tests.Common.Builders
{
    public class SellOrderRequestBuilder
    {
        private string _stockSymbol = "MSFT";
        private string _stockName = "Microsoft Corporation";
        private DateTime _dateAndTimeOfOrder = DateTime.UtcNow;
        private uint _quantity = 10;
        private double _price = 100;

        public SellOrderRequestBuilder WithStockSymbol(string stockSymbol)
        {
            _stockSymbol = stockSymbol;
            return this;
        }

        public SellOrderRequestBuilder WithStockName(string stockName)
        {
            _stockName = stockName;
            return this;
        }

        public SellOrderRequestBuilder WithDateAndTimeOfOrder(DateTime dateAndTimeOfOrder)
        {
            _dateAndTimeOfOrder = dateAndTimeOfOrder;
            return this;
        }

        public SellOrderRequestBuilder WithQuantity(uint quantity)
        {
            _quantity = quantity;
            return this;
        }

        public SellOrderRequestBuilder WithPrice(double price)
        {
            _price = price;
            return this;
        }

        public SellOrderAddRequest Build() => new()
        {
            StockSymbol = _stockSymbol,
            StockName = _stockName,
            DateAndTimeOfOrder = _dateAndTimeOfOrder,
            Quantity = _quantity,
            Price = _price
        };
    }
}
