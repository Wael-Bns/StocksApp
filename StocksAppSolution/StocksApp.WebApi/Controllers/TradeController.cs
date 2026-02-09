using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StocksApp.Core.DTO;
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
    }
}
