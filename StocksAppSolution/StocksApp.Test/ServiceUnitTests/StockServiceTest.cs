using FluentAssertions;
using Moq;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Domain.RepositoryContracts;
using StocksApp.Core.DTO.BuyOrderDTO;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.Exceptions;
using StocksApp.Core.HttpClientAbstractions;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class StockServiceTest
    {
        private readonly IStockService _stockService;
        private readonly IOrderRepository _orderRepository;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IFinnHubHttpClient> _finnHubHttpClientMock;
        // A sample BuyOrderRequest object that can be used in multiple tests
        private BuyOrderRequest buyOrderRequest = new BuyOrderRequest
        {
            StockName = "APPLE INC",
            StockSymbol = "AAPL",
            Quantity = 250,
            DateAndTimeOfOrder = DateTime.Now,
            Price = 100,
        };
        // A sample SellOrderRequest object that can be used in multiple tests
        private SellOrderRequest sellOrderRequest = new SellOrderRequest
        {
            StockName = "APPLE INC",
            StockSymbol = "AAPL",
            Quantity = 250,
            DateAndTimeOfOrder = DateTime.Now,
            Price = 100,
        };

        public StockServiceTest()
        {
            // Mock the IOrderRepository
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _finnHubHttpClientMock = new Mock<IFinnHubHttpClient>();
            _orderRepository = _orderRepositoryMock.Object;
            // Pass the mocked repository to the Service
            _stockService = new StockService(_orderRepository, _finnHubHttpClientMock.Object);
        }
        private void MockAddBuyOrder(BuyOrder buyOrder)
        {
            _orderRepositoryMock.Setup(repo => repo.AddBuyOrderAsync(It.IsAny<BuyOrder>()))
                .ReturnsAsync(buyOrder);
        }
        private void MockAddSellOrder(SellOrder sellOrder)
        {
            _orderRepositoryMock.Setup(repo => repo.AddSellOrderAsync(It.IsAny<SellOrder>()))
                .ReturnsAsync(sellOrder);
        }
        private void MockGetAllBuyOrders(List<BuyOrder> buyOrders)
        {
            _orderRepositoryMock.Setup(repo => repo.GetAllBuyOrdersAsync())
                .ReturnsAsync(buyOrders);
        }
        private void MockGetAllSellOrders(List<SellOrder> sellOrders)
        {
            _orderRepositoryMock.Setup(repo => repo.GetAllSellOrdersAsync())
                .ReturnsAsync(sellOrders);
        }
        private void MockGetStockQuote(StockQuoteDTO? stockQuote)
        {
            _finnHubHttpClientMock.Setup(client => client.GetStockQuote(It.IsAny<string>()))
                .ReturnsAsync(stockQuote);
        }
        private void MockGetCompanyProfile(CompanyProfileDTO? companyProfile)
        {
            _finnHubHttpClientMock.Setup(client => client.GetCompanyProfile(It.IsAny<string>()))
                .ReturnsAsync(companyProfile);
        }

        #region CreateBuyOrder
        [Fact]
        public async Task CreateBuyOrder_NullRequest()
        {
            //Arrange 
            BuyOrderRequest? orderRequest = null;
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(orderRequest);
            };
            await actual.Should().ThrowAsync<ArgumentNullException>();
        }
        [Fact]
        public async Task CreateBuyOrder_OrderQuantityLessThanMinimum()
        {
            // Arrange 
            buyOrderRequest.Quantity = 0;
            MockAddBuyOrder(buyOrderRequest.ToBuyOrder());

            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(buyOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<InvalidPropertyException>();
        }
        [Fact]
        public async Task CreateBuyOrder_OrderQuantityMoreThanMaximum()
        {
            // Arrange 
            buyOrderRequest.Quantity = 10001;

            MockAddBuyOrder(buyOrderRequest.ToBuyOrder());

            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(buyOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<InvalidPropertyException>();
        }
        [Fact]
        public async Task CreateBuyOrder_PriceLessThanMinimum()
        {
            // Arrange 
            buyOrderRequest.Price = 0;
            MockAddBuyOrder(buyOrderRequest.ToBuyOrder());

            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(buyOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<InvalidPropertyException>();
        }
        [Fact]
        public async Task CreateBuyOrder_PriceMoreThanMaximum()
        {
            // Arrange 
            buyOrderRequest.Price = 10001;

            MockAddBuyOrder(buyOrderRequest.ToBuyOrder());

            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(buyOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<InvalidPropertyException>();
        }
        [Fact]
        public async Task CreateBuyOrder_NullStockSymbol()
        {
            // Arrange 
            buyOrderRequest.StockSymbol = null;

            MockAddBuyOrder(buyOrderRequest.ToBuyOrder());
            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(buyOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<InvalidPropertyException>();
        }
        [Fact]
        public async Task CreateBuyOrder_DateLessThanMinimum()
        {
            // Arrange 
            buyOrderRequest.DateAndTimeOfOrder = Convert.ToDateTime("1999-12-31");

            MockAddBuyOrder(buyOrderRequest.ToBuyOrder());
            // Act
            Func<Task> actual = async () =>
            {
                BuyOrderResponse response = await _stockService.CreateBuyOrder(buyOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<InvalidPropertyException>();
        }
        [Fact]
        public async Task CreateBuyOrder_ValidRequest()
        {
            // Arrange
            BuyOrder buyOrder = buyOrderRequest.ToBuyOrder();
            BuyOrderResponse expected = buyOrder.ToBuyOrderResponse();

            MockAddBuyOrder(buyOrder);
            // Act
            BuyOrderResponse actual = await _stockService.CreateBuyOrder(buyOrderRequest);
            // Assert
            actual.Should().BeEquivalentTo(expected);
            actual.BuyOrderID.Should().NotBeEmpty();
        }
        #endregion

        #region CreateSellOrder
        [Fact]
        public async Task CreateSellOrder_NullRequest()
        {
            //Arrange 
            SellOrderRequest? orderRequest = null;
            Func<Task> actual = async () =>
            {
                SellOrderResponse response = await _stockService.CreateSellOrder(orderRequest);
            };
            await actual.Should().ThrowAsync<ArgumentNullException>();
        }
        [Fact]
        public async Task CreateSellOrder_OrderQuantityLessThanMinimum()
        {
            // Arrange 
            sellOrderRequest.Quantity = 0;

            MockAddSellOrder(sellOrderRequest.ToSellOrder());

            // Act
            Func<Task> actual = async () =>
            {
                SellOrderResponse response = await _stockService.CreateSellOrder(sellOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<InvalidPropertyException>();
        }
        [Fact]
        public async Task CreateSellOrder_OrderQuantityMoreThanMaximum()
        {
            // Arrange 
            sellOrderRequest.Quantity = 10001;

            MockAddSellOrder(sellOrderRequest.ToSellOrder());

            // Act
            Func<Task> actual = async () =>
            {
                SellOrderResponse response = await _stockService.CreateSellOrder(sellOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<InvalidPropertyException>();
        }
        [Fact]
        public async Task CreateSellOrder_PriceLessThanMinimum()
        {
            // Arrange 
            sellOrderRequest.Price = 0;

            MockAddSellOrder(sellOrderRequest.ToSellOrder());

            // Act
            Func<Task> actual = async () =>
            {
                SellOrderResponse response = await _stockService.CreateSellOrder(sellOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<InvalidPropertyException>();
        }
        [Fact]
        public async Task CreateSellOrder_PriceMoreThanMaximum()
        {
            // Arrange 
            sellOrderRequest.Price = 10001;

            MockAddSellOrder(sellOrderRequest.ToSellOrder());

            // Act
            Func<Task> actual = async () =>
            {
                SellOrderResponse response = await _stockService.CreateSellOrder(sellOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<InvalidPropertyException>();
        }
        [Fact]
        public async Task CreateSellOrder_NullStockSymbol()
        {
            // Arrange 
            sellOrderRequest.StockSymbol = null;

            MockAddSellOrder(sellOrderRequest.ToSellOrder());
            // Act
            Func<Task> actual = async () =>
            {
                SellOrderResponse response = await _stockService.CreateSellOrder(sellOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<InvalidPropertyException>();
        }
        [Fact]
        public async Task CreateSellOrder_DateLessThanMinimum()
        {
            // Arrange 
            sellOrderRequest.DateAndTimeOfOrder = Convert.ToDateTime("1999-12-31");

            MockAddSellOrder(sellOrderRequest.ToSellOrder());
            // Act
            Func<Task> actual = async () =>
            {
                SellOrderResponse response = await _stockService.CreateSellOrder(sellOrderRequest);
            };
            // Assert
            await actual.Should().ThrowAsync<InvalidPropertyException>();
        }
        [Fact]
        public async Task CreateSellOrder_ValidRequest()
        {
            // Arrange
            SellOrder sellOrder = sellOrderRequest.ToSellOrder();
            SellOrderResponse expected = sellOrder.ToSellOrderResponse();

            MockAddSellOrder(sellOrder);
            // Act
            SellOrderResponse actual = await _stockService.CreateSellOrder(sellOrderRequest);
            // Assert
            actual.Should().BeEquivalentTo(expected);
            actual.SellOrderID.Should().NotBeEmpty();
        }
        #endregion

        #region GetAllBuyOrders
        [Fact]
        public async Task GetAllBuyOrders_Empty()
        {
            // Arrange
            MockGetAllBuyOrders(new List<BuyOrder>());
            // Act
            List<BuyOrderResponse> actual = await _stockService.GetAllBuyOrders();
            // Assert
            actual.Should().BeEmpty();
        }
        [Fact]
        public async Task GetAllBuyOrders_Successful()
        {
            // Arrange 
            List<BuyOrder> buyOrders = [];
            buyOrders.Add(buyOrderRequest.ToBuyOrder());

            MockGetAllBuyOrders(buyOrders);
            
            List<BuyOrderResponse> expected = buyOrders.Select(order => order.ToBuyOrderResponse()).ToList();
            
            // Act
            List<BuyOrderResponse> actual = await _stockService.GetAllBuyOrders();
            
            // Assert
            actual.Should().BeEquivalentTo(expected);

        }
        #endregion

        #region GetAllSellOrders
        [Fact]
        public async Task GetAllSellOrders_Empty()
        {
            // Arrange
            MockGetAllSellOrders(new List<SellOrder>());
            // Act
            List<SellOrderResponse> actual = await _stockService.GetAllSellOrders();
            // Assert
            actual.Should().BeEmpty();
        }
        [Fact]
        public async Task GetAllSellOrders_Successful()
        {
            // Arrange 
            List<SellOrder> sellOrders = [];
            sellOrders.Add(sellOrderRequest.ToSellOrder());

            MockGetAllSellOrders(sellOrders);

            List<SellOrderResponse> expected = sellOrders.Select(order => order.ToSellOrderResponse()).ToList();

            // Act
            List<SellOrderResponse> actual = await _stockService.GetAllSellOrders();

            // Assert
            actual.Should().BeEquivalentTo(expected);

        }
        #endregion

        #region GetStockInformations
        [Fact]
        public async Task GetStockInformations_ValidSymbol_ReturnsStockInformations()
        {
            // Arrange
            string stockSymbol = "AAPL";
            StockQuoteDTO stockQuote = new StockQuoteDTO
            {
                CurrentPrice = 192.53m
            };
            CompanyProfileDTO companyProfile = new CompanyProfileDTO
            {
                Ticker = stockSymbol,
                Name = "Apple Inc",
                Currency = "USD",
                Exchange = "NASDAQ NMS - GLOBAL MARKET",
                WebUrl = "https://www.apple.com/",
                FinnhubIndustry = "Technology",
                Logo = "https://static2.finnhub.io/file/publicdatany/finnhubimage/stock_logo/AAPL.png"
            };
            StockInformations expected = new StockInformations
            {
                StockSymbol = companyProfile.Ticker,
                StockName = companyProfile.Name,
                Currency = companyProfile.Currency,
                Exchange = companyProfile.Exchange,
                WebUrl = companyProfile.WebUrl,
                Industry = companyProfile.FinnhubIndustry,
                PricePerShare = stockQuote.CurrentPrice,
                Logo = companyProfile.Logo
            };

            MockGetStockQuote(stockQuote);
            MockGetCompanyProfile(companyProfile);

            // Act
            StockInformations actual = await _stockService.GetStockInformations(stockSymbol);

            // Assert
            actual.Should().BeEquivalentTo(expected);
            _finnHubHttpClientMock.Verify(client => client.GetStockQuote(stockSymbol), Times.Once);
            _finnHubHttpClientMock.Verify(client => client.GetCompanyProfile(stockSymbol), Times.Once);
        }

        [Fact]
        public async Task GetStockInformations_NullStockQuote_ThrowsStockNotFoundException()
        {
            // Arrange
            string stockSymbol = "INVALID";
            MockGetStockQuote(null);
            MockGetCompanyProfile(new CompanyProfileDTO
            {
                Ticker = stockSymbol,
                Name = "Invalid Stock"
            });

            // Act
            Func<Task> actual = async () => await _stockService.GetStockInformations(stockSymbol);

            // Assert
            await actual.Should().ThrowAsync<StockNotFoundException>()
                .WithMessage($"Stock with symbol '{stockSymbol}' was not found.");
        }

        [Fact]
        public async Task GetStockInformations_ZeroCurrentPrice_ThrowsStockNotFoundException()
        {
            // Arrange
            string stockSymbol = "AAPL";
            MockGetStockQuote(new StockQuoteDTO
            {
                CurrentPrice = 0
            });
            MockGetCompanyProfile(new CompanyProfileDTO
            {
                Ticker = stockSymbol,
                Name = "Apple Inc"
            });

            // Act
            Func<Task> actual = async () => await _stockService.GetStockInformations(stockSymbol);

            // Assert
            await actual.Should().ThrowAsync<StockNotFoundException>();
        }

        [Fact]
        public async Task GetStockInformations_NullCompanyProfile_ThrowsStockNotFoundException()
        {
            // Arrange
            string stockSymbol = "INVALID";
            MockGetStockQuote(new StockQuoteDTO
            {
                CurrentPrice = 192.53m
            });
            MockGetCompanyProfile(null);

            // Act
            Func<Task> actual = async () => await _stockService.GetStockInformations(stockSymbol);

            // Assert
            await actual.Should().ThrowAsync<StockNotFoundException>();
        }
        #endregion

    }
}
