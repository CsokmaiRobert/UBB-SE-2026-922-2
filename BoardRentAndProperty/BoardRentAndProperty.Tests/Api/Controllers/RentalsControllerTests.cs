using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using BoardRentAndProperty.Api.Controllers;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Controllers
{
    [TestFixture]
    public sealed class RentalsControllerTests
    {
        private FakeRentalService rentalService = null!;
        private RentalsController controller = null!;

        [SetUp]
        public void SetUp()
        {
            this.rentalService = new FakeRentalService();
            this.controller = new RentalsController(this.rentalService);
        }

        [Test]
        public void GetForOwnerAndRenter_ReturnRentalsFromService()
        {
            this.rentalService.OwnerRentals = ImmutableList.Create(new RentalDTO { Id = 1 });
            this.rentalService.RenterRentals = ImmutableList.Create(new RentalDTO { Id = 2 });

            Assert.That(this.controller.GetForOwner(Guid.NewGuid()).Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(this.controller.GetForRenter(Guid.NewGuid()).Result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public void Create_WhenServiceSucceeds_ReturnsOkAndForwardsAllArguments()
        {
            var startDate = new DateTime(2030, 1, 1);
            var endDate = startDate.AddDays(3);
            var creationBody = new CreateRentalDataTransferObject
            {
                GameId = 7,
                RenterAccountId = Guid.NewGuid(),
                OwnerAccountId = Guid.NewGuid(),
                StartDate = startDate,
                EndDate = endDate,
            };

            var controllerResponse = this.controller.Create(creationBody);

            Assert.That(controllerResponse, Is.InstanceOf<OkResult>());
            Assert.That(this.rentalService.LastCreatedGameId, Is.EqualTo(7));
        }

        [Test]
        public void Create_HandlesAllErrorPaths()
        {
            this.rentalService.CreateException = new ArgumentException("invalid dates");
            var validationResponse = this.controller.Create(new CreateRentalDataTransferObject());
            Assert.That(((ObjectResult)validationResponse).StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));

            this.rentalService.CreateException = new InvalidOperationException("Dates conflict.");
            var conflictResponse = this.controller.Create(new CreateRentalDataTransferObject());
            Assert.That(((ObjectResult)conflictResponse).StatusCode, Is.EqualTo((int)HttpStatusCode.Conflict));

            this.rentalService.CreateException = new KeyNotFoundException();
            var notFoundResponse = this.controller.Create(new CreateRentalDataTransferObject());
            Assert.That(((ObjectResult)notFoundResponse).StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
        }

        [Test]
        public void CheckSlot_ReturnsOkWithSlotAvailabilityFromService()
        {
            this.rentalService.SlotAvailableOutcome = false;

            var controllerResponse = this.controller.CheckSlot(1, new DateTime(2030, 1, 1), new DateTime(2030, 1, 5));
            var okResult = controllerResponse.Result as OkObjectResult;

            Assert.That(okResult!.Value, Is.EqualTo(false));
        }
    }
}
