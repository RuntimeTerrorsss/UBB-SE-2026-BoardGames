// <copyright file="GamesController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Web.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    [Authorize]
    public class GamesController : Controller
    public class GamesController : Controller
    {
        private readonly IGameProxyService gameProxyService;
        private readonly IRentalProxyService rentalProxyService;

        public GamesController(IGameProxyService gameProxyService, IRentalProxyService rentalProxyService)
        {
            this.gameProxyService = gameProxyService ?? throw new ArgumentNullException(nameof(gameProxyService));
            this.rentalProxyService = rentalProxyService ?? throw new ArgumentNullException(nameof(rentalProxyService));
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            this.ViewBag.IsLoggedIn = this.User?.Identity?.IsAuthenticated == true;
            this.ViewBag.CurrentUsername = this.User?.Identity?.Name;
            this.ViewBag.CurrentDisplayName = this.User?.GetDisplayName();
            base.OnActionExecuting(context);
        }

        public async Task<IActionResult> Index()
        {
            var filter = new FilterCriteria();
            if (this.User?.Identity?.IsAuthenticated == true)
            {
                filter.UserId = this.User.GetPamUserId();
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
        public async Task<IActionResult> ConfirmBooking(int id, DateTime startDate, DateTime endDate, string confirm)
        {
            if (this.User?.Identity?.IsAuthenticated != true)
            {
                return this.RedirectToAction("Login", "Auth");
            }

            int clientId = this.User.GetPamUserId() ?? -1;
            if (clientId == -1)
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