using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StocksApp.Core.DTO.BuyOrderDTO;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.IntegrationsTests.Collection;
using StocksApp.IntegrationsTests.Factory;
using StocksApp.IntegrationsTests.Helpers;
using StocksApp.Tests.Common.Builders;

namespace StocksApp.IntegrationsTests.Tests
{
    [Collection(IntegrationTestsCollection.Name)]
    public class TradeControllerTest : IntegrationTestBase
    {
        private readonly AuthHelper _auth;
        private readonly TradeHelper _trade;

        public TradeControllerTest(CustomWebApplicationFactory factory) : base(factory)
        {
            _auth = new AuthHelper(Client);
            _trade = new TradeHelper(Client);
        }

        private async Task AuthenticateAsync(string email)
        {
            var result = await _auth.RegisterAsync("TraderUser", email);
            _auth.AuthorizeClient(result.Token!);
        }

        [Fact]
        public async Task GetTradeInfo_Unauthenticated_ReturnsUnauthorized()
        {
            var response = await _trade.GetTradeInfoRawAsync("MSFT");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetTradeInfo_Authenticated_ReturnsOk()
        {
            await AuthenticateAsync("tradeinfo@test.com");

            var response = await _trade.GetTradeInfoRawAsync("MSFT");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var stockInfo = await response.Content.ReadFromJsonAsync<StockInformations>();
            stockInfo.Should().NotBeNull();
            stockInfo!.StockSymbol.Should().Be("MSFT");
        }

        [Fact]
        public async Task BuyOrder_Unauthenticated_ReturnsUnauthorized()
        {
            var request = new BuyOrderRequestBuilder().Build();

            var response = await _trade.BuyOrderRawAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task BuyOrder_ValidRequest_ReturnsOkAndCreatesOrder()
        {
            await AuthenticateAsync("buyorder@test.com");

            var request = new BuyOrderRequestBuilder()
                .WithStockSymbol("MSFT")
                .WithStockName("Microsoft Corporation")
                .WithQuantity(10)
                .WithPrice(100)
                .Build();

            var response = await _trade.BuyOrderRawAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var buyOrderResponse = await response.Content.ReadFromJsonAsync<BuyOrderResponse>();
            buyOrderResponse.Should().NotBeNull();
            buyOrderResponse!.StockSymbol.Should().Be("MSFT");
            buyOrderResponse.Quantity.Should().Be(10);
        }

        [Fact]
        public async Task BuyOrder_InvalidRequest_ReturnsBadRequest()
        {
            await AuthenticateAsync("invalidbuy@test.com");

            var request = new BuyOrderRequestBuilder()
                .WithStockSymbol("")
                .WithStockName("")
                .WithQuantity(0)
                .Build();

            var response = await _trade.BuyOrderRawAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SellOrder_Unauthenticated_ReturnsUnauthorized()
        {
            var request = new SellOrderRequestBuilder().Build();

            var response = await _trade.SellOrderRawAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SellOrder_ValidRequest_ReturnsOkAndCreatesOrder()
        {
            await AuthenticateAsync("sellorder@test.com");

            var request = new SellOrderRequestBuilder()
                .WithStockSymbol("MSFT")
                .WithStockName("Microsoft Corporation")
                .WithQuantity(5)
                .WithPrice(100)
                .Build();

            var response = await _trade.SellOrderRawAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var sellOrderResponse = await response.Content.ReadFromJsonAsync<SellOrderResponse>();
            sellOrderResponse.Should().NotBeNull();
            sellOrderResponse!.StockSymbol.Should().Be("MSFT");
            sellOrderResponse.Quantity.Should().Be(5);
        }

        [Fact]
        public async Task SellOrder_InvalidRequest_ReturnsBadRequest()
        {
            await AuthenticateAsync("invalidsell@test.com");

            var request = new SellOrderRequestBuilder()
                .WithStockSymbol("")
                .WithStockName("")
                .WithQuantity(0)
                .Build();

            var response = await _trade.SellOrderRawAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetAllBuyOrders_Unauthenticated_ReturnsUnauthorized()
        {
            var response = await _trade.GetAllBuyOrdersRawAsync();

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllBuyOrders_AfterCreatingOrder_ReturnsOkAndContainsOrder()
        {
            await AuthenticateAsync("allbuyorders@test.com");

            var request = new BuyOrderRequestBuilder()
                .WithStockSymbol("AAPL")
                .WithStockName("Apple Inc.")
                .WithQuantity(2)
                .WithPrice(150)
                .Build();

            await _trade.BuyOrderRawAsync(request);

            var response = await _trade.GetAllBuyOrdersRawAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var buyOrders = await response.Content.ReadFromJsonAsync<List<BuyOrderResponse>>();
            buyOrders.Should().NotBeNull();
            buyOrders!.Should().Contain(o => o.StockSymbol == "AAPL");
        }

        [Fact]
        public async Task GetAllSellOrders_Unauthenticated_ReturnsUnauthorized()
        {
            var response = await _trade.GetAllSellOrdersRawAsync();

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllSellOrders_AfterCreatingOrder_ReturnsOkAndContainsOrder()
        {
            await AuthenticateAsync("allsellorders@test.com");

            var request = new SellOrderRequestBuilder()
                .WithStockSymbol("AAPL")
                .WithStockName("Apple Inc.")
                .WithQuantity(2)
                .WithPrice(150)
                .Build();

            await _trade.SellOrderRawAsync(request);

            var response = await _trade.GetAllSellOrdersRawAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var sellOrders = await response.Content.ReadFromJsonAsync<List<SellOrderResponse>>();
            sellOrders.Should().NotBeNull();
            sellOrders!.Should().Contain(o => o.StockSymbol == "AAPL");
        }
    }
}