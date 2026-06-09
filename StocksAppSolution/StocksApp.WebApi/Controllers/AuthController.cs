using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StocksApp.Core.DTO.AuthenticationDTO;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authenticationService;
        public AuthController(IAuthService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthenticationResponse>> Register([FromBody] UserAddRequest registerRequest)
        {
            AuthenticationResponse response = await _authenticationService.RegisterAsync(registerRequest);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] LoginRequest loginRequest)
        {
            AuthenticationResponse response = await _authenticationService.LoginAsync(loginRequest);
            return Ok(response);
        }

        [HttpPost("generate-new-access-token")]
        public async Task<IActionResult> GenerateNewAccessToken([FromBody] TokenModel tokenModel)
        {
            AuthenticationResponse response = await _authenticationService.GenerateNewAccessTokenAsync(tokenModel);
            return Ok(response);
        }
    }
}
