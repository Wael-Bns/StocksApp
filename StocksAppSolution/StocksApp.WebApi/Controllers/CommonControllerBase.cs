using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StocksApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    [Authorize]
    public class CommonControllerBase : ControllerBase
    {
    }
}
