using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StocksApp.Core.DTO.AuthenticationDTO;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.WebApi.Controllers
{
    public class AuthController : CommonControllerBase
    {
        private readonly IAuthService _authenticationService;
        public AuthController(IAuthService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthenticationResponse>> Register([FromBody] UserAddRequest registerRequest)
        {
            try
            {
                AuthenticationResponse response = await _authenticationService.RegisterAsync(registerRequest);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                AuthenticationResponse response = await _authenticationService.LoginAsync(loginRequest);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost("generate-new-access-token")]
        public async Task<IActionResult> GenerateNewAccessToken([FromBody] TokenModel tokenModel)
        {
            try
            {
                AuthenticationResponse response = await _authenticationService.GenerateNewAccessTokenAsync(tokenModel);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
