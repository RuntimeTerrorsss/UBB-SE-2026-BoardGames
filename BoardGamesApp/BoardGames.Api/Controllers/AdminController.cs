using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Api.Services;
using BoardGames.Shared.Common;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    /* Task 7 must register IAuthService, IAccountService, IAdminService, 
     * IAvatarStorageService, IAccountRepository, 
     * and IFailedLoginRepository in the DI container.
     
     Also, middleware so the "Admin" role is extracted correctly*/
    public class AdminController : ControllerBase
    {
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 100;

        private readonly IAdminService adminService;

        public AdminController(IAdminService adminService)
        {
            this.adminService = adminService;
        }

        [HttpGet("accounts")]
        public async Task<ActionResult<List<AccountProfileDTO>>> GetAccounts(
            [FromQuery] int page = DefaultPageNumber,
            [FromQuery] int pageSize = DefaultPageSize)
        {
            var result = await adminService.GetAllAccountsAsync(page, pageSize);
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return Ok(result.Data);
        }

        [HttpPut("accounts/{accountId:guid}/suspend")]
        public async Task<IActionResult> Suspend(Guid accountId)
        {
            var result = await adminService.SuspendAccountAsync(accountId);
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return NoContent();
        }

        [HttpPut("accounts/{accountId:guid}/unsuspend")]
        public async Task<IActionResult> Unsuspend(Guid accountId)
        {
            var result = await adminService.UnsuspendAccountAsync(accountId);
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return NoContent();
        }

        [HttpPut("accounts/{accountId:guid}/reset-password")]
        public async Task<IActionResult> ResetPassword(Guid accountId, [FromBody] ResetPasswordDTO body)
        {
            var result = await adminService.ResetPasswordAsync(accountId, body.NewPassword);
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return NoContent();
        }

        [HttpPut("accounts/{accountId:guid}/unlock")]
        public async Task<IActionResult> Unlock(Guid accountId)
        {
            var result = await adminService.UnlockAccountAsync(accountId);
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return NoContent();
        }
    }
}
