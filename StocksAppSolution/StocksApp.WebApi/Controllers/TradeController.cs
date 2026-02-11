using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StocksApp.Core.DTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.WebApi.Options;

namespace StocksApp.WebApi.Controllers
{
    public class TradeController : CommonControllerBase
    {
        private readonly IFinnHubService _finnHubService;
        private readonly IStockService _stockService;
        private readonly IOptions<TradeOptions> _tradeOptions;

        public TradeController(IFinnHubService finnHubService, IStockService stockService, IOptions<TradeOptions> tradeOptions)
        {
            _finnHubService = finnHubService;
            _stockService = stockService;
            _tradeOptions = tradeOptions;
        }

        [HttpGet("trade-info/{stockSymbol?}")]
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
                    StockName = companyProfile["name"]?.ToString(),
                    StockSymbol = companyProfile["ticker"]?.ToString(),
                    PricePerShare = stockPriceQuote["c"]?.ToString() != null ? Convert.ToDouble(stockPriceQuote["c"].ToString()) : 0,
                    Logo = companyProfile["logo"]?.ToString(),
                };
                return Ok(stockTrade);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost("[action]")]
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
        [HttpPost("[action]")]
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
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllBuyOrders()
        {
            List<BuyOrderResponse> buyOrders = await _stockService.GetAllBuyOrders();
            return Ok(buyOrders);
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllSellOrders()
        {
            List<SellOrderResponse> sellOrders = await _stockService.GetAllSellOrders();
            return Ok(sellOrders);
        }
    }
}
