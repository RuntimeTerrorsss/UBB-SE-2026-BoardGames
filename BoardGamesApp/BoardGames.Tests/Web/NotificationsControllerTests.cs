//// <copyright file="NotificationsControllerTests.cs" company="BoardRent">
//// Copyright (c) BoardRent. All rights reserved.
//// </copyright>

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using BoardGames.Shared.DTO;
//using BoardGames.Web.Controllers;
//using BoardGames.Web.Infrastructure;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using Xunit;

//namespace BoardGames.Tests.Web
//{
//    public class NotificationsControllerTests
//    {
//        private readonly Mock<INotificationProxyService> notificationProxy;
//        private readonly Guid accountId;

//        public NotificationsControllerTests()
//        {
//            this.notificationProxy = new Mock<INotificationProxyService>();
//            this.accountId = Guid.NewGuid();
//        }

//        private NotificationsController CreateController()
//        {
//            var controller = new NotificationsController(this.notificationProxy.Object);

//            var identity = new ClaimsIdentity(
//                new[]
//            {
//                new Claim(ClaimTypes.NameIdentifier, this.accountId.ToString()),
//            }, "Test");

//            controller.ControllerContext = new ControllerContext
//            {
//                HttpContext = new DefaultHttpContext
//                {
//                    User = new ClaimsPrincipal(identity),
//                },
//            };

//            return controller;
//        }

//        [Fact]
//        public async Task GetCount_ReturnsNotificationCount()
//        {
//            var notifications = new List<NotificationDTO>
//            {
//                new NotificationDTO { Id = 1 },
//                new NotificationDTO { Id = 2 },
//                new NotificationDTO { Id = 3 },
//            };

//            this.notificationProxy
//                .Setup(s => s.GetNotificationsForUserAsync(this.accountId))
//                .ReturnsAsync(notifications);

//            var controller = this.CreateController();

//            var result = await controller.GetCount() as JsonResult;

//            Assert.NotNull(result);
//            var value = result!.Value;
//            var countProperty = value!.GetType().GetProperty("count");
//            Assert.NotNull(countProperty);
//            Assert.Equal(3, countProperty!.GetValue(value));
//        }

//        [Fact]
//        public async Task GetCount_ProxyThrows_ReturnsZero()
//        {
//            this.notificationProxy
//                .Setup(s => s.GetNotificationsForUserAsync(this.accountId))
//                .ThrowsAsync(new ProxyServiceException("fail", HttpStatusCode.InternalServerError, null));

//            var controller = this.CreateController();

//            var result = await controller.GetCount() as JsonResult;

//            Assert.NotNull(result);
//            var value = result!.Value;
//            var countProperty = value!.GetType().GetProperty("count");
//            Assert.NotNull(countProperty);
//            Assert.Equal(0, countProperty!.GetValue(value));
//        }

//        [Fact]
//        public async Task Index_PaginatesCorrectly()
//        {
//            var notifications = Enumerable.Range(1, 7)
//                .Select(i => new NotificationDTO { Id = i })
//                .ToList();

//            this.notificationProxy
//                .Setup(s => s.GetNotificationsForUserAsync(this.accountId))
//                .ReturnsAsync(notifications);

//            var controller = this.CreateController();
//            controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
//                controller.HttpContext, Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

//            var result = await controller.Index(2) as ViewResult;

//            Assert.NotNull(result);
//            var model = Assert.IsAssignableFrom<IEnumerable<NotificationDTO>>(result!.Model);
//            Assert.Equal(3, model.Count());
//            Assert.Equal(2, controller.ViewData["CurrentPage"]);
//            Assert.Equal(7, controller.ViewData["TotalCount"]);
//        }
//    }
//}
