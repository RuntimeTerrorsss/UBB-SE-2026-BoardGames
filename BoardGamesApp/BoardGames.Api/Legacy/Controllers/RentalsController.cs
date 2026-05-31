using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentalsController : ControllerBase
    {
        private const int MinimumValidDayCount = 1;

        private readonly IRentalRepository rentalRepository;

        private readonly IConversationRepository conversationRepository;

        private readonly InterfaceGamesRepository gamesRepository;

        public RentalsController(
            IRentalRepository rentalRepository,
            IConversationRepository conversationRepository,
            InterfaceGamesRepository gamesRepository)
        {
            this.rentalRepository = rentalRepository;
            this.conversationRepository = conversationRepository;
            this.gamesRepository = gamesRepository;
        }

        /// <summary>Creates the rental record and adds a rental-request message to the renter ↔ owner conversation.</summary>
        public record BookGameWithRentalRequestBody(int ClientId, int GameId, DateTime StartDate, DateTime EndDate);

        [HttpGet("{id}")]
        public async Task<ActionResult<Rental>> GetRental(int id)
        {
            var rental = await rentalRepository.GetById(id);
            if (rental == null) return NotFound();
            return Ok(rental);
        }

        [HttpGet("game/{gameId}/unavailable")]
        public async Task<ActionResult<List<TimeRange>>> GetUnavailable(int gameId)
        {
            var list = await rentalRepository.GetUnavailableTimeRanges(gameId);
            return Ok(list);
        }

        [HttpGet("{id}/timerange")]
        public async Task<ActionResult<TimeRange>> GetRentalTimeRange(int id)
        {
            var range = await rentalRepository.GetRentalTimeRange(id);
            if (range == null) return NotFound();
            return Ok(range);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Rental>>> GetRentalsForUser(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("A valid user id is required.");
            }

            var rentals = await rentalRepository.GetRentalsForUser(userId);
            return Ok(rentals);
        }

        [HttpPost("book")]
        public async Task<ActionResult<int>> BookGameWithRentalRequest([FromBody] BookGameWithRentalRequestBody request)
        {
            if (request.ClientId <= 0)
            {
                return BadRequest("A valid renter account is required.");
            }

            request = request with
            {
                StartDate = request.StartDate.Date,
                EndDate = request.EndDate.Date,
            };

            if (request.EndDate < request.StartDate)
            {
                return BadRequest("End date must be on or after the start date.");
            }

            var game = await gamesRepository.GetGameById(request.GameId);
            if (game == null)
            {
                return NotFound($"Game with id {request.GameId} was not found.");
            }

            if (request.ClientId == game.OwnerId)
            {
                return BadRequest("You cannot rent your own game listing.");
            }

            bool available = await rentalRepository.CheckGameAvailability(
                request.StartDate,
                request.EndDate,
                request.GameId);

            if (!available)
            {
                return Conflict("This game is not available for the selected dates.");
            }

            int bookingDays = (request.EndDate - request.StartDate).Days + MinimumValidDayCount;
            if (bookingDays < MinimumValidDayCount)
            {
                bookingDays = MinimumValidDayCount;
            }

            decimal totalPrice = bookingDays * game.PricePerDay;
            var rental = new Rental(
                request.StartDate,
                request.EndDate,
                request.GameId,
                request.ClientId,
                game.OwnerId,
                totalPrice);

            await rentalRepository.AddRental(rental);

            int conversationId = await conversationRepository.FindOrCreateConversationBetweenUsers(
                request.ClientId,
                game.OwnerId);

            string formattedTotal = totalPrice.ToString("0.##", CultureInfo.InvariantCulture);
            string requestSummary =
                $"{game.Name}: {request.StartDate:dd MMM yyyy} – {request.EndDate:dd MMM yyyy}" +
                $" ({bookingDays} day(s), total {formattedTotal}).";

            var rentalRequestMessage = new RentalRequestMessage
            {
                ConversationId = conversationId,
                MessageSenderId = request.ClientId,
                MessageReceiverId = game.OwnerId,
                MessageSentTime = DateTime.Now,
                RentalRequestId = rental.RentalId,
                RequestContent = requestSummary,
                MessageContentAsString = "Rental Request",
                IsRequestResolved = false,
                IsRequestAccepted = false,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            await conversationRepository.HandleNewMessage(rentalRequestMessage);
            return this.Ok(rental.RentalId);
        }

        [HttpPost]
        public async Task<ActionResult> CreateRental([FromBody] Rental rental)
        {
            await rentalRepository.AddRental(rental);
            return Ok(rental.RentalId);
        }

        [HttpPost("{id}/check")]
        public async Task<ActionResult<bool>> CheckAvailability(int id, [FromBody] TimeRange range)
        {
            var available = await rentalRepository.CheckGameAvailability(range.StartTime, range.EndTime, id);
            return Ok(available);
        }
    }
}
