// <copyright file="ProfileController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Web.Controllers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using BoardGames.Shared.DTO;
    using BoardGames.Web.Helpers;
    using BoardGames.Web.Infrastructure;
    using BoardGames.Web.Models.Account;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IAccountProxyService accountProxyService;

        public ProfileController(IAccountProxyService accountProxyService)
        {
            this.accountProxyService = accountProxyService ?? throw new ArgumentNullException(nameof(accountProxyService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                Guid currentUserId = this.User.GetAccountId();
                AccountProfileDTO profileData = await this.accountProxyService.GetProfileAsync(currentUserId);
                ProfileViewModel model = ViewModelAdapter.ToProfileViewModel(profileData);

                return this.View(model);
            }
            catch (ProxyServiceException exception)
            {
                this.TempData["ErrorMessage"] = exception.Message;
                return this.RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                Guid currentUserId = this.User.GetAccountId();
                var profileData = await this.accountProxyService.GetProfileAsync(currentUserId);

                if (!string.IsNullOrEmpty(profileData.AvatarUrl))
                {
                    model.AvatarUrl = profileData.AvatarUrl;
                }

                return this.View(model);
            }

            try
            {
                Guid currentUserId = this.User.GetAccountId();
                AccountProfileDTO updateData = ViewModelAdapter.ToAccountProfileDTO(model);
                await this.accountProxyService.UpdateProfileAsync(currentUserId, updateData);
                this.TempData["SuccessMessage"] = "Profile updated successfully.";
                return this.RedirectToAction(nameof(this.Index));
            }
            catch (ProxyServiceException exception)
            {
                Guid currentUserId = this.User.GetAccountId();
                var profileData = await this.accountProxyService.GetProfileAsync(currentUserId);
                if (!string.IsNullOrEmpty(profileData.AvatarUrl))
                {
                    model.AvatarUrl = profileData.AvatarUrl;
                }

                this.ModelState.AddModelError(string.Empty, exception.Message);
                return this.View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(ProfileViewModel model)
        {
            if (model.AvatarFile == null || model.AvatarFile.Length == 0)
            {
                this.TempData["ErrorMessage"] = "Please select a valid image file.";
                return this.RedirectToAction(nameof(this.Index));
            }

            try
            {
                Guid currentUserId = this.User.GetAccountId();

                string extension = Path.GetExtension(model.AvatarFile.FileName);
                string temporaryPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");

                using (FileStream stream = new FileStream(temporaryPath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                await this.accountProxyService.UploadAvatarAsync(currentUserId, temporaryPath);

                if (System.IO.File.Exists(temporaryPath))
                {
                    System.IO.File.Delete(temporaryPath);
                }

                this.TempData["SuccessMessage"] = "Avatar uploaded successfully.";
            }
            catch (Exception exception)
            {
                this.TempData["ErrorMessage"] = exception.Message;
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAvatar()
        {
            try
            {
                Guid currentUserId = this.User.GetAccountId();
                await this.accountProxyService.RemoveAvatarAsync(currentUserId);
                this.TempData["SuccessMessage"] = "Avatar removed successfully.";
            }
            catch (ProxyServiceException exception)
            {
                this.TempData["ErrorMessage"] = exception.Message;
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return this.View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            try
            {
                Guid currentUserId = this.User.GetAccountId();
                await this.accountProxyService.ChangePasswordAsync(currentUserId, model.CurrentPassword, model.NewPassword);

                this.TempData["SuccessMessage"] = "Password changed successfully. Please login again.";
                return this.RedirectToAction("Login", "Auth");
            }
            catch (ProxyServiceException exception)
            {
                this.ModelState.AddModelError(string.Empty, exception.Message);
                return this.View(model);
            }
        }
    }
}
