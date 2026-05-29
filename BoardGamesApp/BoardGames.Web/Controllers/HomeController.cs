// <copyright file="HomeController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Diagnostics;
using BoardGames.Web.Helpers;
using BoardGames.Web.Models;
using BoardGames.Web.Models.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoardGames.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly InterfaceSearchAndFilterService searchService;

        public HomeController(ILogger<HomeController> loggerParam, InterfaceSearchAndFilterService searchServiceParam)
        {
            this.logger = loggerParam;
            this.searchService = searchServiceParam;
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

            var results = await _searchService.SearchGamesByFilter(filter);
            var distinct = results.DistinctBy(game => game.Name).ToArray();

            var model = new SearchFilterViewModel();
            model.TotalPages = (int)Math.Ceiling(distinct.Length / (double)model.PageSize);
            model.TotalPages = Math.Max(1, model.TotalPages);
            model.Results = distinct.Take(model.PageSize).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Filter(SearchFilterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ErrorMessage = "Please correct the filter values.";
                return View("Index", model);
            }

            if (model.StartDate.HasValue && model.EndDate.HasValue && model.StartDate > model.EndDate)
            {
                model.ErrorMessage = "Start date must be before end date.";
                return View("Index", model);
            }

            var filter = new FilterCriteria
            {
                Name = model.Name,
                City = model.City,
                MaximumPrice = model.MaximumPrice,
                PlayerCount = model.MinimumPlayers,
                UserId = this.User?.Identity?.IsAuthenticated == true ? this.User.GetPamUserId() : null,
                SortOption = model.SortOption switch
                {
                    "price_asc" => SortOption.PriceAscending,
                    "price_desc" => SortOption.PriceDescending,
                    "location" => SortOption.Location,
                    _ => SortOption.None,
                },
                AvailabilityRange = (model.StartDate.HasValue && model.EndDate.HasValue)
                    ? new TimeRange(model.StartDate.Value, model.EndDate.Value)
                    : null,
            };

            var results = await _searchService.SearchGamesByFilter(filter);
            var distinct = results.DistinctBy(game => game.Name).ToArray();

            model.TotalPages = (int)Math.Ceiling(distinct.Length / (double)model.PageSize);
            model.TotalPages = Math.Max(1, model.TotalPages);
            model.Page = Math.Clamp(model.Page, 1, model.TotalPages);

            model.Results = distinct
                .Skip((model.Page - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToList();

            return View("Index", model);
        }

        public IActionResult Privacy()
        {
            return this.View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }
    }
}
