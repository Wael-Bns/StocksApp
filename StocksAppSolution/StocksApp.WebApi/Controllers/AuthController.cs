using Microsoft.AspNetCore.Mvc;
using StocksApp.Core.DTO.UsersDTO;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.WebApi.Controllers
{
    public class AuthController : CommonControllerBase
    {
        private readonly IApplicationUserService _authenticationService;
        public AuthController(IApplicationUserService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost]
        public async Task<ActionResult<ApplicationUserResponse>> Register([FromBody] ApplicationUserRegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var createdUser = await _authenticationService.RegisterUser(registerDTO);
                return CreatedAtAction(nameof(Register), createdUser);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
