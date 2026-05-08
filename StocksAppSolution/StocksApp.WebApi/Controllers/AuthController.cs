using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StocksApp.Core.DTO.AuthenticationDTO;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.WebApi.Controllers
{
    [Route("api/auth")]
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
            catch (ArgumentException ex)
            {
                // Typically "A user with this email already exists."
                return Conflict(ex.Message); 
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
            catch (ArgumentException ex)
            {
                // Thrown for "Invalid email" or "Invalid password"
                return Unauthorized(ex.Message); 
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost("generate-new-access-token")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateNewAccessToken([FromBody] TokenModel tokenModel)
        {
            try
            {
                AuthenticationResponse response = await _authenticationService.GenerateNewAccessTokenAsync(tokenModel);
                return Ok(response);
            }
            catch (SecurityTokenException ex)
            {
                // Invalid or tampered refresh/access tokens
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
