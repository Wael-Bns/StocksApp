using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.DTO.BuyOrderDTO;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Enums;
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
        private readonly ICandleCache _candleCache;

        public TradeControllerTest(CustomWebApplicationFactory factory) : base(factory)
        {
            _auth = new AuthHelper(Client);
            _trade = new TradeHelper(Client);
            _candleCache = Factory.Services.GetRequiredService<ICandleCache>();
        }

        #region Helpers

        private async Task AuthenticateAsync(string email)
        {
            var result = await _auth.RegisterAsync("TraderUser", email);
            _auth.AuthorizeClient(result.Token!);
        }

        #endregion

        #region Get Trade Info

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

        #endregion

        #region Buy Order

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

        #endregion

        #region Sell Order

        [Fact]
        public async Task SellOrder_Unauthenticated_ReturnsUnauthorized()
        {
            var request = new SellOrderAddRequestBuilder().Build();

            var response = await _trade.SellOrderRawAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SellOrder_ValidRequest_ReturnsOkAndCreatesOrder()
        {
            await AuthenticateAsync("sellorder@test.com");

            var request = new SellOrderAddRequestBuilder()
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

            var request = new SellOrderAddRequestBuilder()
                .WithStockSymbol("")
                .WithStockName("")
                .WithQuantity(0)
                .Build();

            var response = await _trade.SellOrderRawAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SellOrder_FreshMarketablePrice_ExecutesImmediately()
        {
            await AuthenticateAsync("immediatefill@test.com");
            const string symbol = "AMZN";
            await _candleCache.SetLatestPriceAsync(new LatestPriceSnapshotBuilder()
                .WithSymbol(symbol)
                .WithPrice(180m)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .Build(), CancellationToken.None);
            var request = new SellOrderAddRequestBuilder()
                .WithStockSymbol(symbol)
                .WithStockName("Amazon.com, Inc.")
                .WithPrice(150)
                .Build();

            var response = await _trade.SellOrderRawAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var sellOrderResponse = await response.Content.ReadFromJsonAsync<SellOrderResponse>();
            sellOrderResponse!.Status.Should().Be(SellOrderStatus.Executed);
        }

        #endregion

        #region Get All Buy Orders

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

        #endregion

        #region Get All Sell Orders

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

            var request = new SellOrderAddRequestBuilder()
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

        #endregion

        #region Cancel Sell Order

        [Fact]
        public async Task CancelSellOrder_Unauthenticated_ReturnsUnauthorized()
        {
            var response = await _trade.CancelSellOrderRawAsync(Guid.NewGuid());

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CancelSellOrder_NonexistentOrder_ReturnsConflict()
        {
            await AuthenticateAsync("cancelmissing@test.com");

            var response = await _trade.CancelSellOrderRawAsync(Guid.NewGuid());

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CancelSellOrder_PendingOrderNoMarketableActivity_ReturnsOkAndMarksCancelled()
        {
            await AuthenticateAsync("cancelpending@test.com");

            var request = new SellOrderAddRequestBuilder()
                .WithStockSymbol("NVDA")
                .WithStockName("NVIDIA Corporation")
                .WithPrice(500)
                .Build();
            var created = await _trade.SellOrderRawAsync(request);
            var order = await created.Content.ReadFromJsonAsync<SellOrderResponse>();
            order!.Status.Should().Be(SellOrderStatus.Pending, "no price was seeded, so it must not have auto-filled");

            var response = await _trade.CancelSellOrderRawAsync(order.SellOrderID);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var all = await (await _trade.GetAllSellOrdersRawAsync()).Content.ReadFromJsonAsync<List<SellOrderResponse>>();
            all!.Single(o => o.SellOrderID == order.SellOrderID).Status.Should().Be(SellOrderStatus.Cancelled);
        }

        [Fact]
        public async Task CancelSellOrder_AlreadyExecutedOrder_ReturnsConflict()
        {
            await AuthenticateAsync("cancelexecuted@test.com");
            const string symbol = "GOOG";
            
            await _candleCache.SetLatestPriceAsync(new LatestPriceSnapshotBuilder()
                .WithSymbol(symbol)
                .WithPrice(150m)
                .WithTimestamp(DateTimeOffset.UtcNow)
                .Build(), CancellationToken.None);

            var request = new SellOrderAddRequestBuilder()
                .WithStockSymbol(symbol)
                .WithStockName("Alphabet Inc.")
                .WithPrice(100)
                .Build();
            var created = await _trade.SellOrderRawAsync(request);
            var order = await created.Content.ReadFromJsonAsync<SellOrderResponse>();
            order!.Status.Should().Be(SellOrderStatus.Executed, "the seeded price must have filled the order immediately");

            var response = await _trade.CancelSellOrderRawAsync(order.SellOrderID);

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CancelSellOrder_NotOwnedByCaller_ReturnsConflict()
        {
            await AuthenticateAsync("owner@test.com");

            var request = new SellOrderAddRequestBuilder()
                .WithStockSymbol("TSLA")
                .WithStockName("Tesla, Inc.")
                .WithPrice(300)
                .Build();
            var created = await _trade.SellOrderRawAsync(request);
            var order = await created.Content.ReadFromJsonAsync<SellOrderResponse>();

            using var intruderClient = Factory.CreateClient();
            var intruderAuth = new AuthHelper(intruderClient);
            var intruderTrade = new TradeHelper(intruderClient);
            var intruderTokens = await intruderAuth.RegisterAsync("Intruder", "intruder@test.com");
            intruderAuth.AuthorizeClient(intruderTokens.Token!);

            var response = await intruderTrade.CancelSellOrderRawAsync(order!.SellOrderID);

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        #endregion
    }
}