// <copyright file="RequestsController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Web.Helpers;
using BoardGames.Web.Infrastructure;
using BoardGames.Web.Models.Games;
using BoardGames.Web.Models.Rentals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    [Authorize]
    public class RequestsController : Controller
    {
        private readonly IRequestProxyService requestProxyService;
        private readonly IGameProxyService gameProxyService;

        public RequestsController(IRequestProxyService requestProxyService, IGameProxyService gameProxyService)
        {
            this.requestProxyService = requestProxyService ?? throw new ArgumentNullException(nameof(requestProxyService));
            this.gameProxyService = gameProxyService ?? throw new ArgumentNullException(nameof(gameProxyService));
        }

        [HttpGet]
        public async Task<IActionResult> My()
        {
            Guid renterAccountId = this.User.GetAccountId();

            try
            {
                var requests = await this.requestProxyService.GetRequestsForRenterAsync(renterAccountId);
                var sortedRequests = requests.OrderByDescending(request => request.StartDate).ToList();

                return this.View(new MyRequestsViewModel
                {
                    Requests = sortedRequests,
                });
            }
            catch (ProxyServiceException ex)
            {
                return this.View(new MyRequestsViewModel
                {
                    ErrorMessage = ex.Message,
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Others()
        {
            Guid ownerAccountId = this.User.GetAccountId();
            var openRequests = await this.requestProxyService.GetOpenRequestsForOwnerAsync(ownerAccountId);
            return View(openRequests);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            Guid renterAccountId = this.User.GetAccountId();

            try
            {
                var availableGames = await this.gameProxyService.GetAvailableGamesForRenterAsync(renterAccountId);

                return this.View(new CreateRequestViewModel
                {
                    AvailableGames = availableGames,
                });
            }
            catch (ProxyServiceException ex)
            {
                return this.View(new CreateRequestViewModel
                {
                    ErrorMessage = ex.Message,
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRequestViewModel form)
        {
            Guid renterAccountId = this.User.GetAccountId();

            var availableGames = await this.LoadAvailableGamesOrEmptyAsync(renterAccountId);

            if (!this.ModelState.IsValid)
            {
                return this.View(new CreateRequestViewModel
                {
                    GameId = form.GameId,
                    StartDate = form.StartDate,
                    EndDate = form.EndDate,
                    AvailableGames = availableGames,
                });
            }

            GameDTO? selectedGame = availableGames.FirstOrDefault(game => game.Id == form.GameId);
            if (selectedGame is null)
            {
                this.ModelState.AddModelError(nameof(form.GameId), "The selected game is not available.");
                return this.View(new CreateRequestViewModel
                {
                    GameId = form.GameId,
                    StartDate = form.StartDate,
                    EndDate = form.EndDate,
                    AvailableGames = availableGames,
                });
            }

            var body = new CreateRequestDTO
            {
                GameId = selectedGame.Id,
                RenterAccountId = renterAccountId,
                OwnerAccountId = selectedGame.Owner?.Id ?? Guid.Empty,
                StartDate = form.StartDate,
                EndDate = form.EndDate,
            };

            try
            {
                await this.requestProxyService.CreateRequestAsync(body);
                return this.RedirectToAction(nameof(this.My));
            }
            catch (ProxyServiceException ex)
            {
                return this.View(new CreateRequestViewModel
                {
                    GameId = form.GameId,
                    StartDate = form.StartDate,
                    EndDate = form.EndDate,
                    AvailableGames = availableGames,
                    ErrorMessage = ex.Message,
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Offer(int id)
        {
            Guid ownerAccountId = this.User.GetAccountId();

            try
            {
                await this.requestProxyService.OfferGameAsync(id, new RequestActionDTO
                {
                    AccountId = ownerAccountId,
                });
                this.TempData["SuccessMessage"] = "The request was approved and the rental was created.";
            }
            catch (ProxyServiceException proxyException)
            {
                this.TempData["ErrorMessage"] = proxyException.Message;
            }

            return this.RedirectToAction(nameof(this.Others));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deny(int id, string? reason)
        {
            Guid ownerAccountId = this.User.GetAccountId();

            try
            {
                await this.requestProxyService.DenyRequestAsync(id, new RequestActionDTO
                {
                    AccountId = ownerAccountId,
                    Reason = reason ?? string.Empty,
                });
                this.TempData["SuccessMessage"] = "The request was declined.";
            }
            catch (ProxyServiceException proxyException)
            {
                this.TempData["ErrorMessage"] = proxyException.Message;
            }

            return this.RedirectToAction(nameof(this.Others));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int requestId)
        {
            Guid renterAccountId = this.User.GetAccountId();

            var body = new RequestActionDTO
            {
                AccountId = renterAccountId,
            };

            try
            {
                await this.requestProxyService.CancelRequestAsync(requestId, body);
            }
            catch (ProxyServiceException ex)
            {
                this.TempData["ErrorMessage"] = ex.Message;
            }

            return this.RedirectToAction(nameof(this.My));
        }

        private async Task<IReadOnlyList<GameDTO>> LoadAvailableGamesOrEmptyAsync(Guid renterAccountId)
        {
            try
            {
                return await this.gameProxyService.GetAvailableGamesForRenterAsync(renterAccountId);
            }
            catch (ProxyServiceException)
            {
                return new List<GameDTO>();
            }
        }
    }
}
