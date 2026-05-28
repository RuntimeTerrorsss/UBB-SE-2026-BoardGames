// <copyright file="SearchController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Web.Infrastructure;
using BoardGames.Web.Models.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly IGameProxyService gameProxyService;

        public SearchController(IGameProxyService gameProxyService)
        {
            this.gameProxyService = gameProxyService ?? throw new ArgumentNullException(nameof(gameProxyService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new SearchFilterViewModel();
            return await this.Filter(model);
        }

        [HttpGet]
        public async Task<IActionResult> Filter(SearchFilterViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                model.ErrorMessage = "Please correct the filter values.";
                return this.View("Index", model);
            }

            if (model.StartDate.HasValue && model.EndDate.HasValue && model.StartDate > model.EndDate)
            {
                model.ErrorMessage = "Start date must be before end date.";
                return this.View("Index", model);
            }

            try
            {
                var criteria = new GameSearchCriteriaDTO
                {
                    Name = model.Name,
                    City = model.City,
                    MaximumPrice = model.MaximumPrice,
                    PlayerCount = model.MinimumPlayers,
                    AvailableFrom = model.StartDate,
                    AvailableTo = model.EndDate,
                    SortBy = model.SortOption,
                };

                var results = await this.gameProxyService.SearchGamesAsync(criteria);
                var distinct = results.DistinctBy(game => game.Name).ToArray();

                model.TotalPages = (int)Math.Ceiling(distinct.Length / (double)model.PageSize);
                model.TotalPages = Math.Max(1, model.TotalPages);
                model.Page = Math.Clamp(model.Page, 1, model.TotalPages);

                model.Results = distinct
                    .Skip((model.Page - 1) * model.PageSize)
                    .Take(model.PageSize)
                    .ToList();

                return this.View("Index", model);
            }
            catch (ProxyServiceException ex)
            {
                model.ErrorMessage = ex.Message;
                model.Results = new List<GameDTO>();
                return this.View("Index", model);
            }
        }
    }
}
