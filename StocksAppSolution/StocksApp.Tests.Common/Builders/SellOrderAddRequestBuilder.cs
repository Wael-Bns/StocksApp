// StocksApp.Tests.Common/Builders/SellOrderAddRequestBuilder.cs
using StocksApp.Core.DTO.SellOrderDTO;

namespace StocksApp.Tests.Common.Builders
{
    public class SellOrderAddRequestBuilder
    {
        private string _stockSymbol = "AAPL";
        private string _stockName = "Apple Inc.";
        private uint _quantity = 10;
        private double _price = 150;

        public SellOrderAddRequestBuilder WithStockSymbol(string symbol) 
        { 
            _stockSymbol = symbol;
            return this; 
        }
        public SellOrderAddRequestBuilder WithStockName(string name) 
        { 
            _stockName = name;
            return this; 
        }
        public SellOrderAddRequestBuilder WithQuantity(uint quantity) 
        { 
            _quantity = quantity;
            return this; 
        }
        public SellOrderAddRequestBuilder WithPrice(double price) 
        { 
            _price = price;
            return this; 
        }

        public SellOrderAddRequest Build() => new()
        {
            StockSymbol = _stockSymbol,
            StockName = _stockName,
            Quantity = _quantity,
            Price = _price
        };
    }
}