//// <copyright file="RequestsControllerTests.cs" company="BoardRent">
//// Copyright (c) BoardRent. All rights reserved.
//// </copyright>

//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using BoardGames.Api.Controllers;
//using BoardGames.Shared.DTO;
//using BoardGames.Web.Controllers;
//using BoardGames.Web.Infrastructure;
//using BoardGames.Web.Models.Requests;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using Xunit;

//namespace BoardGames.Tests.Web
//{
//    public class RequestsControllerTests
//    {
//        private readonly Mock<IRequestProxyService> requestProxy;
//        private readonly Mock<IGameProxyService> gameProxy;
//        private readonly Mock<IChatProxyService> chatProxy;
//        private readonly Mock<IRentalProxyService> rentalProxy;
//        private readonly Guid accountId;

//        private readonly GameDTO availableGame = new GameDTO
//        {
//            Id = 1,
//            Name = "Ticket to Ride",
//            Owner = new UserDTO { Id = Guid.NewGuid(), DisplayName = "Owner" },
//        };

//        public RequestsControllerTests()
//        {
//            this.requestProxy = new Mock<IRequestProxyService>();
//            this.gameProxy = new Mock<IGameProxyService>();
//            this.chatProxy = new Mock<IChatProxyService>();
//            this.rentalProxy = new Mock<IRentalProxyService>();
//            this.accountId = Guid.NewGuid();
//        }

//        private RequestsController CreateController()
//        {
//            var controller = new RequestsController(
//                this.requestProxy.Object,
//                this.gameProxy.Object,
//                this.chatProxy.Object,
//                this.rentalProxy.Object);

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

//        private void SetupAvailableGames(IReadOnlyList<GameDTO>? games = null)
//        {
//            this.gameProxy
//                .Setup(s => s.GetAvailableGamesForRenterAsync(this.accountId, default))
//                .ReturnsAsync(games ?? new List<GameDTO> { this.availableGame });
//        }

//        [Fact]
//        public async Task Create_GamesAvailable_ReturnsViewWithGames()
//        {
//            this.SetupAvailableGames();

//            var result = await this.CreateController().Create() as ViewResult;

//            Assert.NotNull(result);
//            var vm = Assert.IsType<CreateRequestViewModel>(result!.Model);
//            Assert.Single(vm.AvailableGames);
//        }

//        [Fact]
//        public async Task Create_GamesProxyFails_ReturnsViewWithErrorMessage()
//        {
//            this.gameProxy
//                .Setup(s => s.GetAvailableGamesForRenterAsync(this.accountId, default))
//                .ThrowsAsync(new ProxyServiceException("Unavailable", HttpStatusCode.ServiceUnavailable, null));

//            var result = await this.CreateController().Create() as ViewResult;

//            Assert.NotNull(result);
//            var vm = Assert.IsType<CreateRequestViewModel>(result!.Model);
//            Assert.NotNull(vm.ErrorMessage);
//        }

//        [Fact]
//        public async Task Create_StartDateInPast_ReturnsViewWithModelError()
//        {
//            this.SetupAvailableGames();

//            var result = await this.CreateController().Create(new CreateRequestViewModel
//            {
//                GameId = 1,
//                StartDate = DateTime.Today.AddDays(-1),
//                EndDate = DateTime.Today.AddDays(3),
//            }) as ViewResult;

//            Assert.NotNull(result);
//            Assert.True(result!.ViewData.ModelState.ContainsKey(nameof(CreateRequestViewModel.StartDate)));
//        }

//        [Fact]
//        public async Task Create_StartDateInPast_SkipsRequestCreation()
//        {
//            this.SetupAvailableGames();

//            await this.CreateController().Create(new CreateRequestViewModel
//            {
//                GameId = 1,
//                StartDate = DateTime.Today.AddDays(-1),
//                EndDate = DateTime.Today.AddDays(3),
//            });

//            this.requestProxy.Verify(
//                s => s.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), default),
//                Times.Never);
//        }

//        [Fact]
//        public async Task Create_EndDateBeforeStartDate_ReturnsViewWithModelError()
//        {
//            this.SetupAvailableGames();

//            var result = await this.CreateController().Create(new CreateRequestViewModel
//            {
//                GameId = 1,
//                StartDate = DateTime.Today.AddDays(5),
//                EndDate = DateTime.Today.AddDays(2),
//            }) as ViewResult;

//            Assert.NotNull(result);
//            Assert.True(result!.ViewData.ModelState.ContainsKey(nameof(CreateRequestViewModel.StartDate)));
//        }

//        [Fact]
//        public async Task Create_StartDateEqualsEndDate_CreatesRequest()
//        {
//            this.SetupAvailableGames();

//            await this.CreateController().Create(new CreateRequestViewModel
//            {
//                GameId = 1,
//                StartDate = DateTime.Today.AddDays(2),
//                EndDate = DateTime.Today.AddDays(2),
//            });

//            this.requestProxy.Verify(
//                s => s.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), default),
//                Times.Once);
//        }

//        [Fact]
//        public async Task Create_GameNotAvailable_ReturnsViewWithModelError()
//        {
//            this.SetupAvailableGames(new List<GameDTO>());

//            var result = await this.CreateController().Create(new CreateRequestViewModel
//            {
//                GameId = 1,
//                StartDate = DateTime.Today.AddDays(1),
//                EndDate = DateTime.Today.AddDays(3),
//            }) as ViewResult;

//            Assert.NotNull(result);
//            Assert.True(result!.ViewData.ModelState.ContainsKey(nameof(CreateRequestViewModel.GameId)));
//        }

//        [Fact]
//        public async Task Create_GameNotAvailable_SkipsRequestCreation()
//        {
//            this.SetupAvailableGames(new List<GameDTO>());

//            await this.CreateController().Create(new CreateRequestViewModel
//            {
//                GameId = 1,
//                StartDate = DateTime.Today.AddDays(1),
//                EndDate = DateTime.Today.AddDays(3),
//            });

//            this.requestProxy.Verify(
//                s => s.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), default),
//                Times.Never);
//        }

//        [Fact]
//        public async Task Create_ValidForm_RedirectsToChatsIndex()
//        {
//            this.SetupAvailableGames();

//            var result = await this.CreateController().Create(new CreateRequestViewModel
//            {
//                GameId = 1,
//                StartDate = DateTime.Today.AddDays(1),
//                EndDate = DateTime.Today.AddDays(4),
//            }) as RedirectToActionResult;

//            Assert.NotNull(result);
//            Assert.Equal("Index", result!.ActionName);
//            Assert.Equal("Chats", result.ControllerName);
//        }

//        [Fact]
//        public async Task Create_ValidForm_SendsCorrectDataToProxy()
//        {
//            this.SetupAvailableGames();
//            CreateRequestDTO? captured = null;
//            this.requestProxy
//                .Setup(s => s.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), default))
//                .Callback<CreateRequestDTO, System.Threading.CancellationToken>((dto, _) => captured = dto)
//                .Returns(Task.CompletedTask);

//            var start = DateTime.Today.AddDays(1);
//            var end = DateTime.Today.AddDays(5);

//            await this.CreateController().Create(new CreateRequestViewModel
//            {
//                GameId = 1,
//                StartDate = start,
//                EndDate = end,
//            });

//            Assert.NotNull(captured);
//            Assert.Equal(start, captured!.StartDate);
//            Assert.Equal(end, captured.EndDate);
//            Assert.Equal(this.accountId, captured.RenterAccountId);
//        }

//        [Fact]
//        public async Task Create_DatesUnavailable_ReturnsViewWithFriendlyMessage()
//        {
//            this.SetupAvailableGames();
//            this.requestProxy
//                .Setup(s => s.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), default))
//                .ThrowsAsync(new ProxyServiceException("Conflict", HttpStatusCode.Conflict, "dates_unavailable"));

//            var result = await this.CreateController().Create(new CreateRequestViewModel
//            {
//                GameId = 1,
//                StartDate = DateTime.Today.AddDays(1),
//                EndDate = DateTime.Today.AddDays(3),
//            }) as ViewResult;

//            Assert.NotNull(result);
//            var vm = Assert.IsType<CreateRequestViewModel>(result!.Model);
//            Assert.Equal("The selected dates are no longer available.", vm.ErrorMessage);
//        }

//        [Fact]
//        public async Task Create_OwnerCannotRent_ReturnsViewWithFriendlyMessage()
//        {
//            this.SetupAvailableGames();
//            this.requestProxy
//                .Setup(s => s.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), default))
//                .ThrowsAsync(new ProxyServiceException("Bad request", HttpStatusCode.BadRequest, "owner_cannot_rent"));

//            var result = await this.CreateController().Create(new CreateRequestViewModel
//            {
//                GameId = 1,
//                StartDate = DateTime.Today.AddDays(1),
//                EndDate = DateTime.Today.AddDays(3),
//            }) as ViewResult;

//            Assert.NotNull(result);
//            var vm = Assert.IsType<CreateRequestViewModel>(result!.Model);
//            Assert.Equal("You cannot rent your own game.", vm.ErrorMessage);
//        }

//        [Fact]
//        public async Task Create_InvalidDateRange_ReturnsViewWithFriendlyMessage()
//        {
//            this.SetupAvailableGames();
//            this.requestProxy
//                .Setup(s => s.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), default))
//                .ThrowsAsync(new ProxyServiceException("Validation", HttpStatusCode.UnprocessableEntity, "invalid_date_range"));

//            var result = await this.CreateController().Create(new CreateRequestViewModel
//            {
//                GameId = 1,
//                StartDate = DateTime.Today.AddDays(1),
//                EndDate = DateTime.Today.AddDays(3),
//            }) as ViewResult;

//            Assert.NotNull(result);
//            var vm = Assert.IsType<CreateRequestViewModel>(result!.Model);
//            Assert.Equal("Invalid date range.", vm.ErrorMessage);
//        }

//        [Fact]
//        public async Task Create_ProxyFails_PreservesOriginalDatesInView()
//        {
//            this.SetupAvailableGames();
//            this.requestProxy
//                .Setup(s => s.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), default))
//                .ThrowsAsync(new ProxyServiceException("Conflict", HttpStatusCode.Conflict, "dates_unavailable"));

//            var start = DateTime.Today.AddDays(2);
//            var end = DateTime.Today.AddDays(6);

//            var result = await this.CreateController().Create(new CreateRequestViewModel
//            {
//                GameId = 1,
//                StartDate = start,
//                EndDate = end,
//            }) as ViewResult;

//            Assert.NotNull(result);
//            var vm = Assert.IsType<CreateRequestViewModel>(result!.Model);
//            Assert.Equal(start, vm.StartDate);
//            Assert.Equal(end, vm.EndDate);
//        }
//    }
//}