using BoardGames.Api.Services;
using BoardGames.Shared.Common;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }
  
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDataTransferObject body)
        {
            var result = await authService.RegisterAsync(body);
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return Ok(new { result.Data });
        }
   
        [HttpPost("login")]
        public async Task<ActionResult<AccountProfileDataTransferObject>> Login([FromBody] LoginDataTransferObject body)
        {

            var result = await authService.LoginAsync(body);
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return Ok(result.Data);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await authService.LogoutAsync();
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return NoContent();
        }

        [HttpGet("forgot-password")]
        public async Task<ActionResult<string>> ForgotPassword()
        {
            var result = await authService.ForgotPasswordAsync();
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return Ok(result.Data);
        }
    }
}
