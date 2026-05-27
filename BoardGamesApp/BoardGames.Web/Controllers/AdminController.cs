// <copyright file="AdminController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    [Authorize(Roles = AppRoles.Administrator)]
    public class AdminController : Controller
    {
        private readonly IAdminProxyService adminProxyService;

        public AdminController(IAdminProxyService adminProxyServiceParam)
        {
            this.adminProxyService = adminProxyServiceParam;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var accounts = await this.adminProxyService.GetAllAccountsAsync();
            return View(accounts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(string id)
        {
            await this.adminProxyService.SuspendAccountAsync(id);
            this.TempData["SuccessMessage"] = "Account successfully suspended.";
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unsuspend(string id)
        {
            await this.adminProxyService.UnsuspendAccountAsync(id);
            this.TempData["SuccessMessage"] = "Account successfully unsuspended.";
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                this.TempData["ErrorMessage"] = "Password cannot be empty.";
                return this.RedirectToAction(nameof(this.Index));
            }

            await this.adminProxyService.ResetPasswordAsync(id, newPassword);
            this.TempData["SuccessMessage"] = "Password has been successfully reset.";
            return this.RedirectToAction(nameof(this.Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            await this.adminProxyService.UnlockAccountAsync(id);
            this.TempData["SuccessMessage"] = "Account has been successfully unlocked.";
            return this.RedirectToAction(nameof(this.Index));
        }
    }
}
