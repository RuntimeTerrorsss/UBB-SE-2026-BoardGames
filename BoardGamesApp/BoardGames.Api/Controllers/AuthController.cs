// <copyright file="AuthController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Api.Common;
using BoardGames.Api.Services;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Mvc;

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
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return this.Ok(result.Data);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
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
