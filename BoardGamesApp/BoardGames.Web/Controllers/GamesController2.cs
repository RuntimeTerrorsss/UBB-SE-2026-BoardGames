using System;
using System.Threading.Tasks;
using BoardGames.Web.Helpers;
using BoardGames.Web.Infrastructure;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.ProxyServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    [Authorize]
    public class GamesController : Controller
    {
        private readonly IGameProxyService gameProxyService;

        public GamesController(IGameProxyService gameProxyService)
        {
            this.gameProxyService = gameProxyService ?? throw new ArgumentNullException(nameof(gameProxyService));
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsAdministrator())
            {
                var allGames = await gameProxyService.GetAllGamesAsync();
                return View(allGames);
            }

            var ownerId = User.GetAccountId();
            var myGames = await gameProxyService.GetGamesByOwnerAsync(ownerId);
            return View(myGames);
        }

        public async Task<IActionResult> Details(int id)
        {
            GameDTO? game = await gameProxyService.GetGameByIdAsync(id);
            if (game is null)
            {
                return NotFound();
            }

            return View(game);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new GameDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameDTO body, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(body);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);
                body.Image = memoryStream.ToArray();
            }

            body.Owner = new UserDTO
            {
                Id = User.GetAccountId(),
                DisplayName = User.GetDisplayNameOrUsername(),
            };

            try
            {
                await gameProxyService.CreateGameAsync(body);
                return RedirectToAction(nameof(Index));
            }
            catch (ProxyServiceException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(body);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            GameDTO? game = await gameProxyService.GetGameByIdAsync(id);
            if (game is null)
            {
                return NotFound();
            }

            return View(game);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GameDTO body, IFormFile? imageFile)
        {
            if (id != body.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(body);
            }

            GameDTO? existing = await gameProxyService.GetGameByIdAsync(id);
            if (existing is null)
            {
                return NotFound();
            }

            if (!User.IsAdministrator() && existing.Owner.Id != User.GetAccountId())
            {
                return Forbid();
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
                await gameProxyService.UpdateGameAsync(id, body);
                return RedirectToAction(nameof(Index));
            }
            catch (ProxyServiceException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(body);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            GameDTO? game = await gameProxyService.GetGameByIdAsync(id);
            if (game is null)
            {
                return NotFound();
            }

            return View(game);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            GameDTO? existing = await gameProxyService.GetGameByIdAsync(id);
            if (existing is null)
            {
                return NotFound();
            }

            if (!User.IsAdministrator() && existing.Owner.Id != User.GetAccountId())
            {
                return Forbid();
            }

            try
            {
                await gameProxyService.DeleteGameAsync(id);
            }
            catch (ProxyServiceException ex)
            {
                TempData["DeleteError"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
