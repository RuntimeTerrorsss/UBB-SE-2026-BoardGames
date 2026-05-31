using BoardGames.Web.Models.Search;
using BoardGames.Data.Enums;
using BoardGames.Shared.DTO;
using BoardGames.Shared.DTO.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    public class SearchController : BaseController
    {
        private readonly InterfaceSearchAndFilterService searchService;

        public SearchController(InterfaceSearchAndFilterService searchService)
        {
            this.searchService = searchService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var filter = new FilterCriteria();
            if (IsLoggedIn)
            {
                filter.UserId = CurrentUserId;
            }

            var results = await searchService.SearchGamesByFilter(filter);
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
                UserId = IsLoggedIn ? CurrentUserId : null,
                SortOption = model.SortOption switch
                {
                    "price_asc" => SortOption.PriceAscending,
                    "price_desc" => SortOption.PriceDescending,
                    "location" => SortOption.Location,
                    _ => SortOption.None
                },
                AvailabilityRange = model.StartDate.HasValue && model.EndDate.HasValue
                    ? new TimeRange(model.StartDate.Value, model.EndDate.Value)
                    : null
            };

            var results = await searchService.SearchGamesByFilter(filter);
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
    }
}