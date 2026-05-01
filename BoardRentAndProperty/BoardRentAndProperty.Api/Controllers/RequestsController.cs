using System;
using System.Collections.Generic;
using System.Linq;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace BoardRentAndProperty.Api.Controllers
{
    [ApiController]
    [Route("api/requests")]
    public class RequestsController : ControllerBase
    {
        private readonly IRequestService requestService;

        public RequestsController(IRequestService requestService)
        {
            this.requestService = requestService;
        }

        [HttpGet("owner/{ownerAccountId:guid}")]
        public ActionResult<IReadOnlyList<RequestDTO>> GetForOwner(Guid ownerAccountId)
        {
            return Ok(this.requestService.GetRequestsForOwner(ownerAccountId));
        }

        [HttpGet("renter/{renterAccountId:guid}")]
        public ActionResult<IReadOnlyList<RequestDTO>> GetForRenter(Guid renterAccountId)
        {
            return Ok(this.requestService.GetRequestsForRenter(renterAccountId));
        }

        [HttpGet("owner/{ownerAccountId:guid}/open")]
        public ActionResult<IReadOnlyList<RequestDTO>> GetOpenForOwner(Guid ownerAccountId)
        {
            return Ok(this.requestService.GetOpenRequestsForOwner(ownerAccountId));
        }

        [HttpPost]
        public ActionResult<int> Create([FromBody] CreateRequestDataTransferObject body)
        {
            var result = this.requestService.CreateRequest(body.GameId, body.RenterAccountId, body.OwnerAccountId, body.StartDate, body.EndDate);
            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.Error.ToString() });
            }

            return Ok(new { Id = result.Value });
        }

        [HttpPut("{requestId:int}/approve")]
        public ActionResult<int> Approve(int requestId, [FromBody] RequestActionDataTransferObject body)
        {
            var result = this.requestService.ApproveRequest(requestId, body.AccountId);
            if (!result.IsSuccess)
            {
                return MapApproveError(result.Error);
            }

            return Ok(new { RentalId = result.Value });
        }

        [HttpPut("{requestId:int}/deny")]
        public IActionResult Deny(int requestId, [FromBody] RequestActionDataTransferObject body)
        {
            var result = this.requestService.DenyRequest(requestId, body.AccountId, body.Reason ?? string.Empty);
            if (!result.IsSuccess)
            {
                return MapDenyError(result.Error);
            }

            return NoContent();
        }

        [HttpPut("{requestId:int}/cancel")]
        public IActionResult Cancel(int requestId, [FromBody] RequestActionDataTransferObject body)
        {
            var result = this.requestService.CancelRequest(requestId, body.AccountId);
            if (!result.IsSuccess)
            {
                return MapCancelError(result.Error);
            }

            return NoContent();
        }

        [HttpPut("{requestId:int}/offer")]
        public ActionResult<int> Offer(int requestId, [FromBody] RequestActionDataTransferObject body)
        {
            var result = this.requestService.OfferGame(requestId, body.AccountId);
            if (!result.IsSuccess)
            {
                return MapOfferError(result.Error);
            }

            return Ok(new { RentalId = result.Value });
        }

        [HttpGet("games/{gameId:int}/booked-dates")]
        public ActionResult<IReadOnlyList<BookedDateRangeDataTransferObject>> GetBookedDates(int gameId, [FromQuery] int month = 0, [FromQuery] int year = 0)
        {
            var ranges = this.requestService.GetBookedDates(gameId, month, year)
                .Select(range => new BookedDateRangeDataTransferObject
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
            return Ok(this.requestService.CheckAvailability(gameId, startDate, endDate));
        }

        private ActionResult MapApproveError(ApproveRequestError error) =>
            error switch
            {
                ApproveRequestError.NotFound => NotFound(),
                ApproveRequestError.Unauthorized => Forbid(),
                _ => BadRequest(new { Error = error.ToString() }),
            };

        private ActionResult MapDenyError(DenyRequestError error) =>
            error switch
            {
                DenyRequestError.NotFound => NotFound(),
                DenyRequestError.Unauthorized => Forbid(),
                _ => BadRequest(new { Error = error.ToString() }),
            };

        private ActionResult MapCancelError(CancelRequestError error) =>
            error switch
            {
                CancelRequestError.NotFound => NotFound(),
                CancelRequestError.Unauthorized => Forbid(),
                _ => BadRequest(new { Error = error.ToString() }),
            };

        private ActionResult MapOfferError(OfferError error) =>
            error switch
            {
                OfferError.NotFound => NotFound(),
                OfferError.NotOwner => Forbid(),
                _ => BadRequest(new { Error = error.ToString() }),
            };
    }
}
