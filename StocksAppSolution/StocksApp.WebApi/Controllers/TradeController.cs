using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.WebApi.Controllers
{
    public class TradeController : CommonControllerBase
    {
        private readonly IFinnHubService _finnHubService;
        
        public TradeController(IFinnHubService finnHubService)
        {
            _finnHubService = finnHubService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetCompanyProfile([FromQuery] string stockSymbol)
        {
            try
            {
                Dictionary<string, object> companyProfile = await _finnHubService.GetCompanyProfile(stockSymbol);
                var jsonResult = JsonConvert.SerializeObject(companyProfile);
                return Ok(jsonResult);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
