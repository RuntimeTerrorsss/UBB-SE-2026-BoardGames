using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Api.Services;
using BoardGames.Shared.Common;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/requests")]
    public class RequestsController : ControllerBase
    {
        private const int UnspecifiedMonth = 0;
        private const int UnspecifiedYear = 0;

        private readonly IRequestService requestService;

        public RequestsController(IRequestService requestService)
        {
            this.requestService = requestService;
        }

        [HttpGet("owner/{ownerAccountId:guid}")]
        public ActionResult<IReadOnlyList<RequestDTO>> GetForOwner(Guid ownerAccountId)
        {
            return Ok(requestService.GetRequestsForOwner(ownerAccountId));
        }

        [HttpGet("renter/{renterAccountId:guid}")]
        public ActionResult<IReadOnlyList<RequestDTO>> GetForRenter(Guid renterAccountId)
        {
            return Ok(requestService.GetRequestsForRenter(renterAccountId));
        }

        [HttpGet("owner/{ownerAccountId:guid}/open")]
        public ActionResult<IReadOnlyList<RequestDTO>> GetOpenForOwner(Guid ownerAccountId)
        {
            return Ok(requestService.GetOpenRequestsForOwner(ownerAccountId));
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateRequestDTO body)
        {
            var result = await requestService.CreateRequest(body.GameId, body.RenterAccountId, body.OwnerAccountId, body.StartDate, body.EndDate);
            if (!result.IsSuccess)
            {
                return MapCreateError(result.Error);
            }

            return Ok(new { Id = result.Value });
        }

        [HttpPut("{requestId:int}/approve")]
        public async Task<ActionResult<int>> Approve(int requestId, [FromBody] RequestActionDTO body)
        {
            var result = await requestService.ApproveRequest(requestId, body.AccountId);
            if (!result.IsSuccess)
            {
                return MapApproveError(result.Error);
            }

            return Ok(new { RentalId = result.Value });
        }

        [HttpPut("{requestId:int}/deny")]
        public async Task<IActionResult> Deny(int requestId, [FromBody] RequestActionDTO body)
        {
            var result = await requestService.DenyRequest(requestId, body.AccountId, body.Reason ?? string.Empty);
            if (!result.IsSuccess)
            {
                return MapDenyError(result.Error);
            }

            return NoContent();
        }

        [HttpPut("{requestId:int}/cancel")]
        public IActionResult Cancel(int requestId, [FromBody] RequestActionDTO body)
        {
            var result = requestService.CancelRequest(requestId, body.AccountId);
            if (!result.IsSuccess)
            {
                return MapCancelError(result.Error);
            }

            return NoContent();
        }

        [HttpPut("{requestId:int}/offer")]
        public async Task<ActionResult<int>> Offer(int requestId, [FromBody] RequestActionDTO body)
        {
            var result = await requestService.OfferGame(requestId, body.AccountId);
            if (!result.IsSuccess)
            {
                return MapOfferError(result.Error);
            }

            return Ok(new { RentalId = result.Value });
        }

        [HttpGet("games/{gameId:int}/booked-dates")]
        public ActionResult<IReadOnlyList<BookedDateRangeDTO>> GetBookedDates(
            int gameId,
            [FromQuery] int month = UnspecifiedMonth,
            [FromQuery] int year = UnspecifiedYear)
        {
            var ranges = requestService.GetBookedDates(gameId, month, year)
                .Select(range => new BookedDateRangeDTO
                {
                    StartDate = range.StartDate,
                    EndDate = range.EndDate,
                })
                .ToList();
            return Ok(ranges);
        }

        [HttpGet("games/{gameId:int}/availability")]
        public ActionResult<bool> CheckAvailability(int gameId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return Ok(requestService.CheckAvailability(gameId, startDate, endDate));
        }

        private ActionResult MapApproveError(ApproveRequestError error) =>
            error switch
            {
                ApproveRequestError.NotFound => this.ApiNotFound("Request not found.", "request_not_found"),
                ApproveRequestError.Unauthorized => this.ApiForbidden("You are not allowed to approve this request.", "request_forbidden"),
                ApproveRequestError.TransactionFailed => this.ApiConflict("The request could not be approved due to a database error. Please try again.", "request_transaction_failed"),
                _ => this.ApiConflict("The request could not be approved because the underlying data changed.", "request_conflict"),
            };

        private ActionResult MapDenyError(DenyRequestError error) =>
            error switch
            {
                DenyRequestError.NotFound => this.ApiNotFound("Request not found.", "request_not_found"),
                DenyRequestError.Unauthorized => this.ApiForbidden("You are not allowed to deny this request.", "request_forbidden"),
                _ => this.ApiBadRequest(error.ToString()),
            };

        private ActionResult MapCancelError(CancelRequestError error) =>
            error switch
            {
                CancelRequestError.NotFound => this.ApiNotFound("Request not found.", "request_not_found"),
                CancelRequestError.Unauthorized => this.ApiForbidden("You are not allowed to cancel this request.", "request_forbidden"),
                _ => this.ApiBadRequest(error.ToString()),
            };

        private ActionResult MapOfferError(OfferError error) =>
            error switch
            {
                OfferError.NotFound => this.ApiNotFound("Request not found.", "request_not_found"),
                OfferError.NotOwner => this.ApiForbidden("You are not allowed to offer for this request.", "request_forbidden"),
                OfferError.RequestNotOpen => this.ApiConflict("The request is no longer open.", "request_not_open"),
                OfferError.TransactionFailed => this.ApiConflict("The offer could not be completed due to a database error. Please try again.", "request_transaction_failed"),
                _ => this.ApiConflict("The offer could not be completed because the underlying data changed.", "request_conflict"),
            };

        private ActionResult MapCreateError(CreateRequestError error) =>
            error switch
            {
                CreateRequestError.InvalidDateRange => this.ApiValidation("The provided date range is invalid.", "invalid_date_range"),
                CreateRequestError.GameDoesNotExist => this.ApiNotFound("Game not found.", "game_not_found"),
                CreateRequestError.OwnerCannotRent => this.ApiBadRequest("Owner cannot rent their own game.", "owner_cannot_rent"),
                CreateRequestError.DatesUnavailable => this.ApiConflict("The selected dates are unavailable.", "dates_unavailable"),
                _ => this.ApiBadRequest(error.ToString()),
            };
    }
}
