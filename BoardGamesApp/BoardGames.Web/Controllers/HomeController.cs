using BoardGames.Data.Enums;
using BoardGames.Shared.DTO;
using BoardGames.Shared.DTO.Services;
using BoardGames.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BoardGames.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly InterfaceSearchAndFilterService _searchService;

        public HomeController(ILogger<HomeController> logger, InterfaceSearchAndFilterService searchService)
        {
            _logger = logger;
            _searchService = searchService;
        }

        public async Task<IActionResult> Index()
        {
            var filter = new FilterCriteria();
            if (IsLoggedIn)
            {
                filter.UserId = CurrentUserId;
            }

            var games = await _searchService.SearchGamesByFilter(filter);
            return View(games);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
