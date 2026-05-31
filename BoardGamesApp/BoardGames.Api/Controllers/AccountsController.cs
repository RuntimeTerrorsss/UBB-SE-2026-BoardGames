// <copyright file="AccountsController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading.Tasks;
using BoardGames.Api.Services;
using BoardGames.Shared.Common;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountsController : ControllerBase
    {
        private const long MaximumAvatarUploadBytes = 5 * 1024 * 1024;

        private readonly IAccountService accountService;
        private readonly IAvatarStorageService avatarStorageService;

        public AccountsController(IAccountService accountService, IAvatarStorageService avatarStorageService)
        {
            this.accountService = accountService;
            this.avatarStorageService = avatarStorageService;
        }

        [HttpGet("{accountId:guid}")]
        public async Task<ActionResult<AccountProfileDTO>> GetProfile(Guid accountId)
        {
            var result = await this.accountService.GetProfileAsync(accountId);
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return this.Ok(result.Data);
        }

        [HttpPut("{accountId:guid}")]
        public async Task<IActionResult> UpdateProfile(Guid accountId, [FromBody] AccountProfileDTO body)
        {
            var result = await this.accountService.UpdateProfileAsync(accountId, body);
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return this.NoContent();
        }

        [HttpPut("{accountId:guid}/password")]
        public async Task<IActionResult> ChangePassword(Guid accountId, [FromBody] ChangePasswordDTO body)
        {
            var result = await this.accountService.ChangePasswordAsync(accountId, body.CurrentPassword, body.NewPassword);
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return this.NoContent();
        }

        [HttpPost("{accountId:guid}/avatar")]
        public async Task<ActionResult<AvatarUploadResponseDTO>> UploadAvatar(Guid accountId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return this.ApiValidation("File is required.", "avatar_file_required");
            }

            if (file.Length > MaximumAvatarUploadBytes)
            {
                return this.ApiValidation("File is too large. Maximum allowed size is 5 MB.", "avatar_file_too_large");
            }

            string extension = Path.GetExtension(file.FileName);
            string relativeUrl;
            await using (var stream = file.OpenReadStream())
            {
                relativeUrl = await this.avatarStorageService.SaveAsync(accountId, stream, extension);
            }

            var result = await this.accountService.SetAvatarUrlAsync(accountId, relativeUrl);
            if (!result.Success)
            {
                this.avatarStorageService.Delete(relativeUrl);
                return this.FromServiceError(result.Error);
            }

            return this.Ok(new AvatarUploadResponseDTO { AvatarUrl = relativeUrl });
        }

        [HttpDelete("{accountId:guid}/avatar")]
        public async Task<IActionResult> RemoveAvatar(Guid accountId)
        {
            var result = await this.accountService.RemoveAvatarAsync(accountId);
            if (!result.Success)
            {
                return this.FromServiceError(result.Error);
            }

            return this.NoContent();
        }
    }
}
