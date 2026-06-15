using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StocksApp.Core.DTO.BuyOrderDTO;
using StocksApp.Core.DTO.SellOrderDTO;
using StocksApp.Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace StocksApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TradeController : ControllerBase
    {
        private readonly IStockService _stockService;

        public TradeController(IStockService stockService)
        {
            _stockService = stockService;
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized();
            }

            buyOrderRequest.UserId = userId;
           
            BuyOrderResponse buyOrderResponse = await _stockService.CreateBuyOrder(buyOrderRequest);
            return Ok(buyOrderResponse);
        }
        [HttpPost("sellorder")]
        public async Task<IActionResult> SellOrder(SellOrderAddRequest sellOrderRequest)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized();
            }

            sellOrderRequest.UserId = userId;
            
            SellOrderResponse sellOrderResponse = await _stockService.CreateSellOrder(sellOrderRequest);
            return Ok(sellOrderResponse);
        }
        [HttpGet("allbuyorders")]
        public async Task<IActionResult> GetAllBuyOrders()
        {
            List<BuyOrderResponse> buyOrders = await _stockService.GetAllBuyOrders();
            return Ok(buyOrders);
        }
        [HttpGet("allsellorders")]
        public async Task<IActionResult> GetAllSellOrders()
        {
            List<SellOrderResponse> sellOrders = await _stockService.GetAllSellOrders();
            return Ok(sellOrders);
        }
    }
}
