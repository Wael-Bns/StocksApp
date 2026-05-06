using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StocksApp.Core.DTO;
using StocksApp.Core.DTO.BuyOrderDTO;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.WebApi.Constants;
using StocksApp.WebApi.Options;
using StocksApp.Core.HttpClientAbstractions;

namespace StocksApp.WebApi.Controllers
{
    public class TradeController : CommonControllerBase
    {
        private readonly IFinnHubHttpClient _finnHubService;
        private readonly IStockService _stockService;
        private readonly IOptions<TradeOptions> _tradeOptions;

        public TradeController(IFinnHubHttpClient finnHubService, IStockService stockService, IOptions<TradeOptions> tradeOptions)
        {
            _finnHubService = finnHubService;
            _stockService = stockService;
            _tradeOptions = tradeOptions;
        }

        [HttpGet]
        public async Task<IActionResult> SearchStocks(string query)
        {
            var res = await _finnHubService.SearchStocks(query);
            return Ok(res);
        }

        [HttpGet("{stockSymbol?}")]
        public async Task<IActionResult> GetTradeInfo([FromRoute] string? stockSymbol)
        {
            try
            {
                if (stockSymbol == null)
                {
                    stockSymbol = _tradeOptions.Value.DefaultStockSymbol;
                }
                var companyProfile = await _finnHubService.GetCompanyProfile(stockSymbol);
                var stockPriceQuote = await _finnHubService.GetStockPriceQuote(stockSymbol);
                if (companyProfile == null || stockPriceQuote == null)
                {
                    return NotFound($"No trade information found for stock symbol: {stockSymbol}");
                }
                var stockTrade = new StockTrade
                {
                    StockName = companyProfile[FinnhubConstants.Name]?.ToString(),
                    StockSymbol = companyProfile[FinnhubConstants.Ticker]?.ToString(),
                    PricePerShare = stockPriceQuote[FinnhubConstants.CurrentPrice]?.ToString() != null ? Convert.ToDouble(stockPriceQuote[FinnhubConstants.CurrentPrice].ToString()) : 0,
                    Logo = companyProfile[FinnhubConstants.Logo]?.ToString(),
                };
                return Ok(stockTrade);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuyOrder(BuyOrderRequest buyOrderRequest)
        {
            if (buyOrderRequest == null)
            {
                return BadRequest("Buy order request cannot be null.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            BuyOrderResponse buyOrderResponse = await _stockService.CreateBuyOrder(buyOrderRequest);
            return Ok(buyOrderResponse);
        }
        [HttpPost]
        public async Task<IActionResult> SellOrder(SellOrderRequest sellOrderRequest)
        {
            if (sellOrderRequest == null)
            {
                return BadRequest("Sell order request cannot be null.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            SellOrderResponse sellOrderResponse = await _stockService.CreateSellOrder(sellOrderRequest);
            return Ok(sellOrderResponse);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllBuyOrders()
        {
            List<BuyOrderResponse> buyOrders = await _stockService.GetAllBuyOrders();
            return Ok(buyOrders);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllSellOrders()
        {
            List<SellOrderResponse> sellOrders = await _stockService.GetAllSellOrders();
            return Ok(sellOrders);
        }
    }
}
