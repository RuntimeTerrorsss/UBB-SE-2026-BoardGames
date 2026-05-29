

using System.Security.Claims;
using BoardGames.Api.Services;
using BoardGames.Shared.Common;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        public const string PamUserIdClaimType = "pam_user_id";

        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO body)
        {
            var result = await this.authService.RegisterAsync(body);
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return this.Ok(new { result.Data });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AccountProfileDTO>> Login([FromBody] LoginDTO body)
        {
            var result = await this.authService.LoginAsync(body);
            if (!result.Success || result.Data is null)
            {
                return this.FromServiceError(result.Error);
            }

            var profile = result.Data;
            string roleName = profile.Role?.Name ?? "Standard User";
            string authorizationRole = string.Equals(roleName, "Administrator", System.StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : roleName;

            var claims = new System.Collections.Generic.List<Claim>
            {
                new(ClaimTypes.NameIdentifier, profile.Id.ToString()),
                new(ClaimTypes.Name, profile.Username ?? string.Empty),
                new(ClaimTypes.Role, authorizationRole),
            };

            if (profile.PamUserId.HasValue)
            {
                claims.Add(new Claim(PamUserIdClaimType, profile.PamUserId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return this.Ok(profile);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var result = await this.authService.LogoutAsync();
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return this.NoContent();
        }

        [HttpGet("forgot-password")]
        public async Task<ActionResult<string>> ForgotPassword()
        {
            var result = await this.authService.ForgotPasswordAsync();
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return this.Ok(result.Data);
        }
    }
}
