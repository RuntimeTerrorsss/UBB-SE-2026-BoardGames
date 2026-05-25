// <copyright file="HomeController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Diagnostics;
using BoardGames.Data.Enums;
using BoardGames.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> logger;
        private readonly InterfaceSearchAndFilterService searchService;

        public HomeController(ILogger<HomeController> loggerParam, InterfaceSearchAndFilterService searchServiceParam)
        {
            this.logger = loggerParam;
            this.searchService = searchServiceParam;
        }

        public async Task<IActionResult> Index()
        {
            var filter = new FilterCriteria();
            if (IsLoggedIn)
            {
                filter.UserId = CurrentUserId;
            }

            var games = await this.searchService.SearchGamesByFilter(filter);
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
