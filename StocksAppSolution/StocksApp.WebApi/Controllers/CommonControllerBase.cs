using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StocksApp.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    public class CommonControllerBase : ControllerBase
    {
    }
}
