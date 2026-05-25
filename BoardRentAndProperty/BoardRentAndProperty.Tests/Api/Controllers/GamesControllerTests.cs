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
    public sealed class GamesControllerTests
    {
        private FakeGameService gameService = null!;
        private GamesController controller = null!;

        [SetUp]
        public void SetUp()
        {
            this.gameService = new FakeGameService();
            this.controller = new GamesController(this.gameService);
        }

        [Test]
        public void GetAll_ReturnsOkWithEntireGameList()
        {
            this.gameService.AllGames = ImmutableList.Create(new GameDTO { Id = 1 }, new GameDTO { Id = 2 });

            var controllerResponse = this.controller.GetAll();
            var okResult = controllerResponse.Result as OkObjectResult;
            var returnedGames = okResult!.Value as ImmutableList<GameDTO>;

            Assert.That(returnedGames!, Has.Count.EqualTo(2));
        }

        [Test]
        public void GetById_WhenServiceThrowsKeyNotFound_ReturnsNotFound()
        {
            this.gameService.GetByIdentifierException = new KeyNotFoundException();

            var controllerResponse = this.controller.GetById(99);
            var notFoundResult = controllerResponse.Result as ObjectResult;

            Assert.That(notFoundResult!.StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
        }

        [Test]
        public void GetByOwner_GetActiveByOwner_GetAvailableForRenter_AllReturnOk()
        {
            this.gameService.OwnerGames = ImmutableList.Create(new GameDTO { Id = 10 });
            this.gameService.ActiveOwnerGames = ImmutableList.Create(new GameDTO { Id = 11 });
            this.gameService.AvailableRenterGames = ImmutableList.Create(new GameDTO { Id = 12 });

            Assert.That(this.controller.GetByOwner(Guid.NewGuid()).Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(this.controller.GetActiveByOwner(Guid.NewGuid()).Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(this.controller.GetAvailableForRenter(Guid.NewGuid()).Result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public void Create_WhenServiceSucceeds_ReturnsOkAndForwardsPayload()
        {
            var newGame = new GameDTO { Name = "Pandemic" };

            var controllerResponse = this.controller.Create(newGame);

            Assert.That(controllerResponse, Is.InstanceOf<OkResult>());
            Assert.That(this.gameService.LastAddedGame, Is.SameAs(newGame));
        }

        [Test]
        public void Create_WhenServiceThrowsArgumentException_ReturnsValidationError()
        {
            this.gameService.AddException = new ArgumentException("Invalid input");

            var controllerResponse = this.controller.Create(new GameDTO());

            Assert.That(((ObjectResult)controllerResponse).StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
        }

        [Test]
        public void Update_HandlesSuccessAndAllErrorPaths()
        {
            var noContentResponse = this.controller.Update(42, new GameDTO());
            Assert.That(noContentResponse, Is.InstanceOf<NoContentResult>());

            this.gameService.UpdateException = new ArgumentException("must be at least 1");
            var validationResponse = this.controller.Update(5, new GameDTO());
            Assert.That(((ObjectResult)validationResponse).StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));

            this.gameService.UpdateException = new KeyNotFoundException();
            var notFoundResponse = this.controller.Update(5, new GameDTO());
            Assert.That(((ObjectResult)notFoundResponse).StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
        }

        [Test]
        public void Delete_HandlesSuccessAndAllErrorPaths()
        {
            var deletedGame = new GameDTO { Id = 33, Name = "Removed" };
            this.gameService.DeletedGame = deletedGame;
            var okResponse = this.controller.Delete(33);
            Assert.That(((OkObjectResult)okResponse.Result!).Value, Is.SameAs(deletedGame));

            this.gameService.DeleteException = new InvalidOperationException("Active rentals exist.");
            var conflictResponse = this.controller.Delete(33);
            Assert.That(((ObjectResult)conflictResponse.Result!).StatusCode, Is.EqualTo((int)HttpStatusCode.Conflict));

            this.gameService.DeleteException = new KeyNotFoundException();
            var notFoundResponse = this.controller.Delete(404);
            Assert.That(((ObjectResult)notFoundResponse.Result!).StatusCode, Is.EqualTo((int)HttpStatusCode.NotFound));
        }
    }
}
