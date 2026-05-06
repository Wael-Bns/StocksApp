using Microsoft.AspNetCore.Authentication;
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
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<AuthenticationResponse>> Register([FromBody] UserAddRequest registerRequest)
        {
            try
            {
                AuthenticationResponse authenticationResponse = await _authenticationService.RegisterAsync(registerRequest);
                return Ok(authenticationResponse);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
