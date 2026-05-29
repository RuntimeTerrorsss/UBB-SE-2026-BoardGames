// <copyright file="NotificationsController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using BoardGames.Api.Services;
using BoardGames.Shared.Common;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        [HttpGet("user/{accountId:guid}")]
        public ActionResult<IReadOnlyList<NotificationDTO>> GetForUser(Guid accountId)
        {
            return this.Ok(this.notificationService.GetNotificationsForUser(accountId));
        }

        [HttpGet("{notificationId:int}")]
        public ActionResult<NotificationDTO> GetById(int notificationId)
        {
            try
            {
                return this.Ok(this.notificationService.GetNotificationByIdentifier(notificationId));
            }
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound("Notification not found.", "notification_not_found");
            }
        }

        [HttpPut("{notificationId:int}")]
        public IActionResult Update(int notificationId, [FromBody] NotificationDTO body)
        {
            try
            {
                this.notificationService.UpdateNotificationByIdentifier(notificationId, body);
                return this.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound("Notification not found.", "notification_not_found");
            }
        }

        [HttpDelete("{notificationId:int}")]
        public ActionResult<NotificationDTO> Delete(int notificationId)
        {
            try
            {
                return this.Ok(this.notificationService.DeleteNotificationByIdentifier(notificationId));
            }
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound("Notification not found.", "notification_not_found");
            }
        }
    }
}
