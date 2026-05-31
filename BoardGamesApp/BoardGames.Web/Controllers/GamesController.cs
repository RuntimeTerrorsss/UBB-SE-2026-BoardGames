using System;
using BoardGames.Web.Helpers;
using BoardGames.Data.Enums;
using BoardGames.Shared.DTO;
using BoardGames.Shared.Mapper;
using BoardGames.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    [Authorize]
    public class GamesController : BaseController
    {
        private readonly InterfaceBookingService _bookingService;
        private readonly InterfaceSearchAndFilterService _searchService;

        public GamesController(InterfaceBookingService bookingService, InterfaceSearchAndFilterService searchService)
        {
            _bookingService = bookingService;
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

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingService.GetBookingInformationForSpecificGame(id);
            if (booking == null)
            {
                return NotFound();
            }

            var unavailableRanges = await _bookingService.GetUnavailableTimeRanges(id);
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

            var booking = await _bookingService.GetBookingInformationForSpecificGame(id);
            if (booking == null)
            {
                return NotFound();
            }

            var timeRange = new TimeRange(startDate, endDate);
            bool isAvailable = await _bookingService.CheckGameAvailability(id, timeRange);

            if (!isAvailable)
            {
                TempData["Error"] = "The game is not available for the selected period.";
                return RedirectToAction("Details", new { id });
            }

            decimal totalPrice = _bookingService.CalculateTotalPriceForRentingASpecificGame(booking.Price, timeRange);
            int totalDays = _bookingService.CalculateNumberOfDaysInAGivenTimeRange(timeRange);

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.TotalPrice = totalPrice;
            ViewBag.TotalDays = totalDays;

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(int id, DateTime startDate, DateTime endDate, string confirm)
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            int clientId = CurrentUserId ?? -1;
            if (clientId == -1) return Unauthorized();

            startDate = startDate.Date;
            endDate = endDate.Date;

            var timeRange = new TimeRange(startDate, endDate);
            var booking = await _bookingService.GetBookingInformationForSpecificGame(id);
            if (booking == null) return NotFound();

            try
            {
                await _bookingService.AddBooking(id, clientId, timeRange);
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
