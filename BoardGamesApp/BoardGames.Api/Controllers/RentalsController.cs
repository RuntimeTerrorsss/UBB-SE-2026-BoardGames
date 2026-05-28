using System;
using System.Collections.Generic;
using BoardGames.Api.Services;
using BoardGames.Shared.Common;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/rentals")]
    public class RentalsController : ControllerBase
    {
        private readonly IRentalService rentalService;

        public RentalsController(IRentalService rentalService)
        {
            this.rentalService = rentalService;
        }

        [HttpGet("owner/{ownerAccountId:guid}")]
        public ActionResult<IReadOnlyList<RentalDTO>> GetForOwner(Guid ownerAccountId)
        {
            return Ok(rentalService.GetRentalsForOwner(ownerAccountId));
        }

        [HttpGet("renter/{renterAccountId:guid}")]
        public ActionResult<IReadOnlyList<RentalDTO>> GetForRenter(Guid renterAccountId)
        {
            return Ok(rentalService.GetRentalsForRenter(renterAccountId));
        }

        /// <summary>
        /// Internal/admin route only. Normal user flow must go through POST api/requests instead.
        /// </summary>
        [HttpPost]
        public IActionResult Create([FromBody] CreateRentalDTO body)
        {
            try
            {
                rentalService.CreateConfirmedRental(body.GameId, body.RenterAccountId, body.OwnerAccountId, body.StartDate, body.EndDate);
                return Ok();
            }
            catch (ArgumentException exception)
            {
                return this.ApiValidation(exception.Message, "rental_validation_failed");
            }
            catch (InvalidOperationException exception)
            {
                return this.ApiConflict(exception.Message, "rental_conflict");
            }
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound("Game not found.", "game_not_found");
            }
        }

        [HttpGet("games/{gameId:int}/booked-dates")]
        public ActionResult<IReadOnlyList<BookedDateRangeDTO>> GetBookedDates(int gameId)
        {
            return Ok(rentalService.GetBookedDatesForGame(gameId));
        }

        [HttpGet("games/{gameId:int}/availability")]
        public ActionResult<bool> CheckSlot(int gameId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return Ok(rentalService.IsSlotAvailable(gameId, startDate, endDate));
        }
    }
}
