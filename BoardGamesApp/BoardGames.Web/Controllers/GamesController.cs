// <copyright file="GamesController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Enums;
using BoardGames.Web.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    [Authorize]
    public class GamesController : Controller
    {
        private readonly InterfaceBookingService bookingService;
        private readonly InterfaceSearchAndFilterService searchService;

        public GamesController(InterfaceBookingService bookingServiceParam, InterfaceSearchAndFilterService searchServiceParam)
        {
            this.bookingService = bookingServiceParam;
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

            var games = await this.searchService.SearchGamesByFilter(filter);
            return View(games);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var booking = await this.bookingService.GetBookingInformationForSpecificGame(id);
            if (booking == null)
            {
                return NotFound();
            }

            var unavailableRanges = await this.bookingService.GetUnavailableTimeRanges(id);
            ViewBag.UnavailableRanges = unavailableRanges;
            booking = booking with
            {
                ImageUrl = GameImageMapper.GetImageUrl(booking.Name),
                AvatarUrl = MediaUrlHelper.ResolveUserImageUrl(booking.AvatarUrl),
            };
            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmBooking(int id, DateTime startDate, DateTime endDate)
        {
            startDate = startDate.Date;
            endDate = endDate.Date;

            var booking = await this.bookingService.GetBookingInformationForSpecificGame(id);
            if (booking == null)
            {
                return NotFound();
            }

            var timeRange = new TimeRange(startDate, endDate);
            bool isAvailable = await this.bookingService.CheckGameAvailability(id, timeRange);

            if (!isAvailable)
            {
                TempData["Error"] = "The game is not available for the selected period.";
                return RedirectToAction("Details", new { id });
            }

            decimal totalPrice = this.bookingService.CalculateTotalPriceForRentingASpecificGame(booking.Price, timeRange);
            int totalDays = this.bookingService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.TotalPrice = totalPrice;
            ViewBag.TotalDays = totalDays;

            return View(booking);
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
                return Unauthorized();
            }

            startDate = startDate.Date;
            endDate = endDate.Date;

            var timeRange = new TimeRange(startDate, endDate);
            var booking = await this.bookingService.GetBookingInformationForSpecificGame(id);
            if (booking == null)
            {
                return NotFound();
            }

            try
            {
                await this.bookingService.AddBooking(id, clientId, timeRange);
            }
            catch (Exception)
            {
                TempData["Error"] = "This game is not available for the selected period.";
                return RedirectToAction("Details", new { id });
            }

            TempData["Success"] = "Rental request sent! The owner can accept it in Messages.";
            return RedirectToAction("Index", "Chats");
        }
    }
}
