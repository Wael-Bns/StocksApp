using StocksApp.Core.DTO.BuyOrderDTO;

namespace StocksApp.IntegrationsTests.ObjectsBuilders
{
    public class BuyOrderRequestBuilder
    {
        private string _stockSymbol = "MSFT";
        private string _stockName = "Microsoft Corporation";
        private DateTime _dateAndTimeOfOrder = DateTime.UtcNow;
        private uint _quantity = 10;
        private double _price = 100;

        public BuyOrderRequestBuilder WithStockSymbol(string stockSymbol)
        {
            _stockSymbol = stockSymbol;
            return this;
        }

        public BuyOrderRequestBuilder WithStockName(string stockName)
        {
            _stockName = stockName;
            return this;
        }

        public BuyOrderRequestBuilder WithDateAndTimeOfOrder(DateTime dateAndTimeOfOrder)
        {
            _dateAndTimeOfOrder = dateAndTimeOfOrder;
            return this;
        }

        public BuyOrderRequestBuilder WithQuantity(uint quantity)
        {
            _quantity = quantity;
            return this;
        }

        public BuyOrderRequestBuilder WithPrice(double price)
        {
            _price = price;
            return this;
        }

        public BuyOrderAddRequest Build() => new()
        {
            StockSymbol = _stockSymbol,
            StockName = _stockName,
            DateAndTimeOfOrder = _dateAndTimeOfOrder,
            Quantity = _quantity,
            Price = _price
        };
    }
}