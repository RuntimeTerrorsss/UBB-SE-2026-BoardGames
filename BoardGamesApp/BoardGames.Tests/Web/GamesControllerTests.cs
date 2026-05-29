// <copyright file="GamesControllerTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;
using BoardGames.Web.Controllers;
using BoardGames.Web.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BoardGames.Tests.Web
{
    public class GamesControllerTests
    {
        private readonly Mock<IGameProxyService> gameProxy;
        private readonly Mock<IRentalProxyService> rentalProxy;
        private readonly Guid accountId;

        public GamesControllerTests()
        {
            this.gameProxy = new Mock<IGameProxyService>();
            this.rentalProxy = new Mock<IRentalProxyService>();
            this.accountId = Guid.NewGuid();
        }

        private GamesController CreateController()
        {
            var controller = new GamesController(this.gameProxy.Object, this.rentalProxy.Object);

            var identity = new ClaimsIdentity(
                new[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.accountId.ToString()),
            }, "Test");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity),
                },
            };

            return controller;
        }

        [Fact]
        public async Task Details_GameExists_ReturnsViewWithGame()
        {
            var game = new GameDTO { Id = 1, Name = "Catan" };

            this.gameProxy
                .Setup(s => s.GetGameByIdAsync(1, default))
                .ReturnsAsync(game);
            this.rentalProxy
                .Setup(s => s.GetBookedDatesForGameAsync(1, default))
                .ReturnsAsync(new List<BookedDateRangeDTO>());

            var result = await this.CreateController().Details(1) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal(game, result!.Model);
        }

        [Fact]
        public async Task Details_GameExists_ReturnsBookedDatesInViewBag()
        {
            var game = new GameDTO { Id = 1, Name = "Catan" };
            var bookedDates = new List<BookedDateRangeDTO>
            {
                new BookedDateRangeDTO { StartDate = DateTime.Today.AddDays(5), EndDate = DateTime.Today.AddDays(8) },
            };

            this.gameProxy
                .Setup(s => s.GetGameByIdAsync(1, default))
                .ReturnsAsync(game);
            this.rentalProxy
                .Setup(s => s.GetBookedDatesForGameAsync(1, default))
                .ReturnsAsync(bookedDates);

            var result = await this.CreateController().Details(1) as ViewResult;

            Assert.NotNull(result);
            var viewBagDates = result!.ViewData["BookedDates"] as IReadOnlyList<BookedDateRangeDTO>;
            Assert.NotNull(viewBagDates);
            Assert.Single(viewBagDates!);
        }

        [Fact]
        public async Task Details_NoBookingsExist_ReturnsEmptyBookedDatesInViewBag()
        {
            var game = new GameDTO { Id = 1, Name = "Catan" };

            this.gameProxy
                .Setup(s => s.GetGameByIdAsync(1, default))
                .ReturnsAsync(game);
            this.rentalProxy
                .Setup(s => s.GetBookedDatesForGameAsync(1, default))
                .ReturnsAsync(new List<BookedDateRangeDTO>());

            var result = await this.CreateController().Details(1) as ViewResult;

            Assert.NotNull(result);
            var viewBagDates = result!.ViewData["BookedDates"] as IReadOnlyList<BookedDateRangeDTO>;
            Assert.NotNull(viewBagDates);
            Assert.Empty(viewBagDates!);
        }

        [Fact]
        public async Task Details_GameNotFound_ReturnsNotFound()
        {
            this.gameProxy
                .Setup(s => s.GetGameByIdAsync(99, default))
                .ReturnsAsync((GameDTO?)null);

            var result = await this.CreateController().Details(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_GameNotFound_SkipsBookedDatesCall()
        {
            this.gameProxy
                .Setup(s => s.GetGameByIdAsync(99, default))
                .ReturnsAsync((GameDTO?)null);

            await this.CreateController().Details(99);

            this.rentalProxy.Verify(
                s => s.GetBookedDatesForGameAsync(It.IsAny<int>(), default),
                Times.Never);
        }

        [Fact]
        public async Task Details_BookedDatesProxyFails_ReturnsEmptyBookedDatesInViewBag()
        {
            var game = new GameDTO { Id = 1, Name = "Pandemic" };

            this.gameProxy
                .Setup(s => s.GetGameByIdAsync(1, default))
                .ReturnsAsync(game);
            this.rentalProxy
                .Setup(s => s.GetBookedDatesForGameAsync(1, default))
                .ThrowsAsync(new ProxyServiceException("Service unavailable", HttpStatusCode.ServiceUnavailable, null));

            var result = await this.CreateController().Details(1) as ViewResult;

            Assert.NotNull(result);
            var viewBagDates = result!.ViewData["BookedDates"] as IReadOnlyList<BookedDateRangeDTO>;
            Assert.NotNull(viewBagDates);
            Assert.Empty(viewBagDates!);
        }

        [Fact]
        public async Task Details_BookedDatesProxyFails_ReturnsGameInView()
        {
            var game = new GameDTO { Id = 1, Name = "Pandemic" };

            this.gameProxy
                .Setup(s => s.GetGameByIdAsync(1, default))
                .ReturnsAsync(game);
            this.rentalProxy
                .Setup(s => s.GetBookedDatesForGameAsync(1, default))
                .ThrowsAsync(new ProxyServiceException("Service unavailable", HttpStatusCode.ServiceUnavailable, null));

            var result = await this.CreateController().Details(1) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal(game, result!.Model);
        }
    }
}