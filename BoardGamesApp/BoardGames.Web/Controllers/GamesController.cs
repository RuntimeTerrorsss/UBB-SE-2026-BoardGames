// <copyright file="GamesController.cs" company="BoardRent">
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
    public class GamesController : Controller
    {
        private readonly IGameProxyService gameProxyService;
        private readonly IRentalProxyService rentalProxyService;
        private readonly IRequestProxyService requestProxyService;

        public GamesController(IGameProxyService gameProxyService, IRentalProxyService rentalProxyService, IRequestProxyService requestProxyService)
        {
            this.gameProxyService = gameProxyService ?? throw new ArgumentNullException(nameof(gameProxyService));
            this.rentalProxyService = rentalProxyService ?? throw new ArgumentNullException(nameof(rentalProxyService));
            this.requestProxyService = requestProxyService ?? throw new ArgumentNullException(nameof(requestProxyService));
        }

        public async Task<IActionResult> Index()
        {
            if (this.User.IsAdministrator())
            {
                var allGames = await this.gameProxyService.GetAllGamesAsync();
                return this.View(allGames);
            }

            var ownerId = this.User.GetAccountId();
            var myGames = await this.gameProxyService.GetGamesByOwnerAsync(ownerId);
            return this.View(myGames);
        }

        public async Task<IActionResult> Details(int id)
        {
            GameDTO? game = await this.gameProxyService.GetGameByIdAsync(id);
            if (game is null)
            {
                return this.NotFound();
            }

            try
            {
                var bookedDates = await this.rentalProxyService.GetBookedDatesForGameAsync(id);
                this.ViewBag.BookedDates = bookedDates;
            }
            catch (ProxyServiceException)
            {
                this.ViewBag.BookedDates = new List<BookedDateRangeDTO>();
            }

            return this.View(game);
        }

        [HttpGet]
        public async Task<IActionResult> Book(int id, DateTime startDate, DateTime endDate)
        {
            if (startDate == default || endDate == default || endDate < startDate)
            {
                return this.RedirectToAction(nameof(this.Details), new { id });
            }

            GameDTO? game = await this.gameProxyService.GetGameByIdAsync(id);
            if (game is null)
            {
                return this.NotFound();
            }

            this.ViewBag.StartDate = startDate;
            this.ViewBag.EndDate = endDate;
            return this.View(game);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int id, DateTime startDate, DateTime endDate, string unused = "")
        {
            GameDTO? game = await this.gameProxyService.GetGameByIdAsync(id);
            if (game is null)
            {
                return this.NotFound();
            }

            Guid renterAccountId = this.User.GetAccountId();
            var body = new CreateRequestDTO
            {
                GameId = game.Id,
                RenterAccountId = renterAccountId,
                OwnerAccountId = game.Owner?.Id ?? Guid.Empty,
                StartDate = startDate,
                EndDate = endDate,
            };

            try
            {
                await this.requestProxyService.CreateRequestAsync(body);
                this.TempData["SuccessMessage"] = "Your rental request has been submitted!";
                return this.RedirectToAction("Index", "Chats");
            }
            catch (ProxyServiceException ex)
            {
                this.ViewBag.StartDate = startDate;
                this.ViewBag.EndDate = endDate;
                this.ViewBag.ErrorMessage = ex.Message;
                return this.View(game);
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return this.View(new GameDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameDTO body, IFormFile? imageFile)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(body);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);
                body.Image = memoryStream.ToArray();
            }

            body.Owner = new UserDTO
            {
                Id = this.User.GetAccountId(),
                DisplayName = this.User.GetDisplayNameOrUsername(),
            };

            try
            {
                await this.gameProxyService.CreateGameAsync(body);
                return this.RedirectToAction(nameof(this.Index));
            }
            catch (ProxyServiceException ex)
            {
                this.ModelState.AddModelError(string.Empty, ex.Message);
                return this.View(body);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            GameDTO? game = await this.gameProxyService.GetGameByIdAsync(id);
            if (game is null)
            {
                return this.NotFound();
            }

            return this.View(game);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GameDTO body, IFormFile? imageFile)
        {
            if (id != body.Id)
            {
                return this.NotFound();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(body);
            }

            GameDTO? existing = await this.gameProxyService.GetGameByIdAsync(id);
            if (existing is null)
            {
                return this.NotFound();
            }

            if (!this.User.IsAdministrator() && existing.Owner?.Id != this.User.GetAccountId())
            {
                return this.Forbid();
            }

            body.Owner = existing.Owner;

            if (imageFile != null && imageFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);
                body.Image = memoryStream.ToArray();
            }
            else
            {
                body.Image = existing.Image;
            }

            try
            {
                await this.gameProxyService.UpdateGameAsync(id, body);
                return this.RedirectToAction(nameof(this.Index));
            }
            catch (ProxyServiceException ex)
            {
                this.ModelState.AddModelError(string.Empty, ex.Message);
                return this.View(body);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            GameDTO? game = await this.gameProxyService.GetGameByIdAsync(id);
            if (game is null)
            {
                return this.NotFound();
            }

            return this.View(game);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            GameDTO? existing = await this.gameProxyService.GetGameByIdAsync(id);
            if (existing is null)
            {
                return this.NotFound();
            }

            if (!this.User.IsAdministrator() && existing.Owner?.Id != this.User.GetAccountId())
            {
                return this.Forbid();
            }

            try
            {
                await this.gameProxyService.DeleteGameAsync(id);
            }
            catch (ProxyServiceException ex)
            {
                this.TempData["DeleteError"] = ex.Message;
            }

            return this.RedirectToAction(nameof(this.Index));
        }
    }
}