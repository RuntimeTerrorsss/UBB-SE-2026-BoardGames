// <copyright file="RequestsControllerCreateTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Net;
using System.Security.Claims;
using BoardGames.Shared.DTO;
using BoardGames.Web.Controllers;
using BoardGames.Web.Infrastructure;
using BoardGames.Web.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace BoardGames.Tests.Web
{
    public class RequestsControllerCreateTests
    {
        private static readonly Guid TestRenterAccountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly Guid TestOwnerAccountId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        private readonly Mock<IRequestProxyService> requestProxy;
        private readonly Mock<IGameProxyService> gameProxy;
        private readonly Mock<IChatProxyService> chatProxy;

        public RequestsControllerCreateTests()
        {
            this.requestProxy = new Mock<IRequestProxyService>();
            this.gameProxy = new Mock<IGameProxyService>();
            this.chatProxy = new Mock<IChatProxyService>();
        }

        // ───────────────────────────────────────────────
        // Success path
        // ───────────────────────────────────────────────

        [Fact]
        public async Task Create_Post_Success_RedirectsToChatsIndex()
        {
            var controller = this.BuildController();
            this.SetupAvailableGames();

            var form = this.BuildValidForm();

            var result = await controller.Create(form) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);
            Assert.Equal("Chats", result.ControllerName);
        }

        [Fact]
        public async Task Create_Post_Success_SetsTempDataSuccessMessage()
        {
            var controller = this.BuildController();
            this.SetupAvailableGames();

            var form = this.BuildValidForm();

            await controller.Create(form);

            Assert.True(controller.TempData.ContainsKey("SuccessMessage"));
            Assert.Equal("Your rental request has been submitted successfully!", controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Create_Post_Success_CallsCreateRequestAsync()
        {
            var controller = this.BuildController();
            this.SetupAvailableGames();

            var form = this.BuildValidForm();

            await controller.Create(form);

            this.requestProxy.Verify(
                service => service.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ───────────────────────────────────────────────
        // API error code mapping
        // ───────────────────────────────────────────────

        [Theory]
        [InlineData("owner_cannot_rent", "You cannot rent your own game.")]
        [InlineData("dates_unavailable", "The selected dates are no longer available.")]
        [InlineData("game_not_found", "Game not found.")]
        [InlineData("invalid_date_range", "Invalid date range.")]
        public async Task Create_Post_ApiErrorCode_ShowsFriendlyMessage(string apiErrorCode, string expectedMessage)
        {
            var controller = this.BuildController();
            this.SetupAvailableGames();
            this.SetupCreateThrows(apiErrorCode, HttpStatusCode.BadRequest);

            var form = this.BuildValidForm();

            var result = await controller.Create(form) as ViewResult;

            Assert.NotNull(result);
            var viewModel = Assert.IsType<CreateRequestViewModel>(result!.Model);
            Assert.Equal(expectedMessage, viewModel.ErrorMessage);
        }

        [Fact]
        public async Task Create_Post_ApiErrorCode_OwnerCannotRent_PreservesFormValues()
        {
            var controller = this.BuildController();
            this.SetupAvailableGames();
            this.SetupCreateThrows("owner_cannot_rent", HttpStatusCode.BadRequest);

            var form = this.BuildValidForm();

            var result = await controller.Create(form) as ViewResult;

            Assert.NotNull(result);
            var viewModel = Assert.IsType<CreateRequestViewModel>(result!.Model);
            Assert.Equal(form.GameId, viewModel.GameId);
            Assert.Equal(form.StartDate, viewModel.StartDate);
            Assert.Equal(form.EndDate, viewModel.EndDate);
            Assert.NotNull(viewModel.AvailableGames);
            Assert.NotEmpty(viewModel.AvailableGames);
        }

        [Fact]
        public async Task Create_Post_UnknownApiError_FallsBackToExceptionMessage()
        {
            var controller = this.BuildController();
            this.SetupAvailableGames();

            this.requestProxy
                .Setup(service => service.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ProxyServiceException("Something unexpected happened.", HttpStatusCode.InternalServerError, null));

            var form = this.BuildValidForm();

            var result = await controller.Create(form) as ViewResult;

            Assert.NotNull(result);
            var viewModel = Assert.IsType<CreateRequestViewModel>(result!.Model);
            Assert.Equal("Something unexpected happened.", viewModel.ErrorMessage);
        }

        [Fact]
        public async Task Create_Post_ConflictWithoutCode_ShowsConflictMessage()
        {
            var controller = this.BuildController();
            this.SetupAvailableGames();

            this.requestProxy
                .Setup(service => service.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ProxyServiceException("conflict detail", HttpStatusCode.Conflict, null));

            var form = this.BuildValidForm();

            var result = await controller.Create(form) as ViewResult;

            Assert.NotNull(result);
            var viewModel = Assert.IsType<CreateRequestViewModel>(result!.Model);
            Assert.Equal("The request could not be completed due to a conflict. Please try again.", viewModel.ErrorMessage);
        }

        [Fact]
        public async Task Create_Post_NotFoundWithoutCode_ShowsNotFoundMessage()
        {
            var controller = this.BuildController();
            this.SetupAvailableGames();

            this.requestProxy
                .Setup(service => service.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ProxyServiceException("not found detail", HttpStatusCode.NotFound, null));

            var form = this.BuildValidForm();

            var result = await controller.Create(form) as ViewResult;

            Assert.NotNull(result);
            var viewModel = Assert.IsType<CreateRequestViewModel>(result!.Model);
            Assert.Equal("The requested resource was not found.", viewModel.ErrorMessage);
        }

        // ───────────────────────────────────────────────
        // Client-side validation
        // ───────────────────────────────────────────────

        [Fact]
        public async Task Create_Post_StartDateInPast_ReturnsViewWithModelError()
        {
            var controller = this.BuildController();
            this.SetupAvailableGames();

            var form = this.BuildValidForm();
            form.StartDate = DateTime.Today.AddDays(-1);

            var result = await controller.Create(form) as ViewResult;

            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(nameof(CreateRequestViewModel.StartDate)));
            this.requestProxy.Verify(
                service => service.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Create_Post_StartDateAfterEndDate_ReturnsViewWithModelError()
        {
            var controller = this.BuildController();
            this.SetupAvailableGames();

            var form = this.BuildValidForm();
            form.StartDate = DateTime.Today.AddDays(5);
            form.EndDate = DateTime.Today.AddDays(2);

            var result = await controller.Create(form) as ViewResult;

            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
            this.requestProxy.Verify(
                service => service.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Create_Post_GameNotInAvailableList_ReturnsViewWithModelError()
        {
            var controller = this.BuildController();
            this.SetupAvailableGames();

            var form = this.BuildValidForm();
            form.GameId = 9999;

            var result = await controller.Create(form) as ViewResult;

            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(nameof(CreateRequestViewModel.GameId)));
            this.requestProxy.Verify(
                service => service.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ───────────────────────────────────────────────
        // Helpers
        // ───────────────────────────────────────────────

        private RequestsController BuildController()
        {
            var controller = new RequestsController(
                this.requestProxy.Object,
                this.gameProxy.Object,
                this.chatProxy.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, TestRenterAccountId.ToString()),
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal },
            };

            controller.TempData = new TempDataDictionary(
                controller.ControllerContext.HttpContext,
                Mock.Of<ITempDataProvider>());

            return controller;
        }

        private void SetupAvailableGames()
        {
            var games = new List<GameDTO>
            {
                new GameDTO
                {
                    Id = 1,
                    Name = "Catan",
                    Price = 15m,
                    City = "Cluj",
                    MinimumPlayerNumber = 3,
                    MaximumPlayerNumber = 4,
                    Owner = new UserDTO { Id = TestOwnerAccountId, DisplayName = "Owner" },
                },
            };

            this.gameProxy
                .Setup(service => service.GetAvailableGamesForRenterAsync(TestRenterAccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(games);
        }

        private void SetupCreateThrows(string apiErrorCode, HttpStatusCode statusCode)
        {
            this.requestProxy
                .Setup(service => service.CreateRequestAsync(It.IsAny<CreateRequestDTO>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ProxyServiceException("API error", statusCode, apiErrorCode));
        }

        private CreateRequestViewModel BuildValidForm()
        {
            return new CreateRequestViewModel
            {
                GameId = 1,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(3),
            };
        }
    }
}
