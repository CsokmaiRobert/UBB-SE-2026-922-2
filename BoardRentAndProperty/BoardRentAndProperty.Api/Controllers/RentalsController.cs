using System;
using System.Collections.Generic;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace BoardRentAndProperty.Api.Controllers
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
            return Ok(this.rentalService.GetRentalsForOwner(ownerAccountId));
        }

        [HttpGet("renter/{renterAccountId:guid}")]
        public ActionResult<IReadOnlyList<RentalDTO>> GetForRenter(Guid renterAccountId)
        {
            return Ok(this.rentalService.GetRentalsForRenter(renterAccountId));
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateRentalDataTransferObject body)
        {
            try
            {
                this.rentalService.CreateConfirmedRental(body.GameId, body.RenterAccountId, body.OwnerAccountId, body.StartDate, body.EndDate);
                return Ok();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { Error = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(new { Error = exception.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("games/{gameId:int}/availability")]
        public ActionResult<bool> CheckSlot(int gameId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return Ok(this.rentalService.IsSlotAvailable(gameId, startDate, endDate));
        }
    }
}
