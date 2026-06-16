using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StocksApp.Core.DTO.BuyOrderDTO;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.WebApi.Options;
using Microsoft.AspNetCore.Authorization;

namespace StocksApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TradeController : ApiControllerBase
    {
        private readonly IStockService _stockService;
        private readonly IOptions<TradeOptions> _tradeOptions;

        public TradeController(IStockService stockService, IOptions<TradeOptions> tradeOptions)
        {
            _stockService = stockService;
            _tradeOptions = tradeOptions;
        }

        [HttpGet("trade-info/{stockSymbol=MSFT}")]
        public async Task<IActionResult> GetTradeInfo([FromRoute] string stockSymbol)
        {
            var stockInformations = await _stockService.GetStockInformations(stockSymbol);
            return Ok(stockInformations);
        }

        [HttpPost("buyorder")]
        public async Task<IActionResult> BuyOrder(BuyOrderAddRequest buyOrderRequest)
        {
            BuyOrderResponse buyOrderResponse = await _stockService.CreateBuyOrder(buyOrderRequest, CurrentUserId);
            return Ok(buyOrderResponse);
        }
        [HttpPost("sellorder")]
        public async Task<IActionResult> SellOrder(SellOrderAddRequest sellOrderRequest)
        {
            SellOrderResponse sellOrderResponse = await _stockService.CreateSellOrder(sellOrderRequest, CurrentUserId);
            return Ok(sellOrderResponse);
        }
        [HttpGet("allbuyorders")]
        public async Task<IActionResult> GetAllBuyOrders()
        {
            List<BuyOrderResponse> buyOrders = await _stockService.GetBuyOrdersByUser(CurrentUserId);
            return Ok(buyOrders);
        }
        [HttpGet("allsellorders")]
        public async Task<IActionResult> GetAllSellOrders()
        {
            List<SellOrderResponse> sellOrders = await _stockService.GetSellOrdersByUser(CurrentUserId);
            return Ok(sellOrders);
        }
    }
}
