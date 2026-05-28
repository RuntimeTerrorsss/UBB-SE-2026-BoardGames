// <copyright file="NotificationsController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Web.Helpers;
using BoardGames.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationProxyService notificationProxyService;

        public NotificationsController(INotificationProxyService notificationProxyService)
        {
            this.notificationProxyService = notificationProxyService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            Guid accountId = this.User.GetAccountId();
            int pageSize = 3;

            try
            {
                var notifications = await this.notificationProxyService.GetNotificationsForUserAsync(accountId);

                var paginatedNotifications = notifications.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                this.ViewData["CurrentPage"] = page;
                this.ViewData["TotalCount"] = notifications.Count;
                this.ViewData["PageSize"] = pageSize;

                return View(paginatedNotifications);
            }
            catch (ProxyServiceException exception)
            {
                this.TempData["ErrorMessage"] = "Could not load notifications at this time.";
                return this.View(Array.Empty<NotificationDTO>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            Guid accountId = this.User.GetAccountId();

            try
            {
                var notifications = await this.notificationProxyService.GetNotificationsForUserAsync(accountId);
                return this.Json(new { count = notifications.Count });
            }
            catch (ProxyServiceException)
            {
                return this.Json(new { count = 0 });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await this.notificationProxyService.DeleteNotificationAsync(id);
                this.TempData["SuccessMessage"] = "Notification deleted successfully.";
            }
            catch (ProxyServiceException exception)
            {
                this.TempData["ErrorMessage"] = "Failed to delete notification. " + exception.Message;
            }

            return this.RedirectToAction(nameof(this.Index));
        }
    }
}
