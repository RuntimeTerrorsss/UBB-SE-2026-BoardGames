// <copyright file="RentalsController2.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Api.Common;
using BoardGames.Api.Services;
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
            return this.Ok(this.rentalService.GetRentalsForOwner(ownerAccountId));
        }

        [HttpGet("renter/{renterAccountId:guid}")]
        public ActionResult<IReadOnlyList<RentalDTO>> GetForRenter(Guid renterAccountId)
        {
            return this.Ok(this.rentalService.GetRentalsForRenter(renterAccountId));
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateRentalDTO body)
        {
            try
            {
                this.rentalService.CreateConfirmedRental(body.GameId, body.RenterAccountId, body.OwnerAccountId, body.StartDate, body.EndDate);
                return this.Ok();
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

        [HttpGet("games/{gameId:int}/availability")]
        public ActionResult<bool> CheckSlot(int gameId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return this.Ok(this.rentalService.IsSlotAvailable(gameId, startDate, endDate));
        }
    }
}
