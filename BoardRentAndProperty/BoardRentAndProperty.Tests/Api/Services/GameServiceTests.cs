using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Services
{
    [TestFixture]
    public sealed class GameServiceTests
    {
        private const int SampleGameIdentifier = 42;

        private readonly Guid sampleOwnerIdentifier = Guid.NewGuid();
        private FakeGameRepository gameRepository = null!;
        private FakeRentalRepository rentalRepository = null!;
        private FakeApiRequestService requestService = null!;
        private GameService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.gameRepository = new FakeGameRepository();
            this.rentalRepository = new FakeRentalRepository();
            this.requestService = new FakeApiRequestService();

            this.service = new GameService(
                this.gameRepository,
                this.rentalRepository,
                new GameMapper(new UserMapper()),
                this.requestService);
        }

        [Test]
        public void DeleteGameByIdentifier_WithOneActiveRental_ThrowsInvalidOperationException()
        {
            var activeRental = new Rental(
                1,
                new Game { Id = SampleGameIdentifier },
                new Account { Id = Guid.NewGuid(), DisplayName = "Renter" },
                new Account { Id = this.sampleOwnerIdentifier, DisplayName = "Owner" },
                DateTime.Now.AddDays(-1),
                DateTime.Now.AddDays(3));

            this.rentalRepository.RentalsByGame = ImmutableList.Create(activeRental);

            Action deleteAction = () => this.service.DeleteGameByIdentifier(SampleGameIdentifier);

            var exception = Assert.Throws<InvalidOperationException>(() => deleteAction());
            Assert.That(exception!.Message, Does.Contain("1 active rental"));
        }

        [Test]
        public void AddGame_WithValidDto_CallsRepositoryAddOnce()
        {
            var gameDto = new GameDTO
            {
                Id = SampleGameIdentifier,
                Name = "Chess Classic",
                Price = 15m,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
                Description = "A classic strategy board game for two players.",
                Owner = new UserDTO { Id = this.sampleOwnerIdentifier, DisplayName = "Owner" },
            };

            this.service.AddGame(gameDto);

            Assert.That(this.gameRepository.AddCallCount, Is.EqualTo(1));
            Assert.That(this.gameRepository.LastAddedGame, Is.Not.Null);
        }

        [Test]
        public void AddGame_WithInvalidDto_ThrowsArgumentException()
        {
            var gameDto = new GameDTO
            {
                Id = SampleGameIdentifier,
                Name = string.Empty,
                Price = 0m,
                MinimumPlayerNumber = 0,
                MaximumPlayerNumber = 0,
                Description = string.Empty,
            };

            Action addAction = () => this.service.AddGame(gameDto);

            Assert.Throws<ArgumentException>(() => addAction());
        }

        [Test]
        public void DeleteGameByIdentifier_WithMultipleActiveRentals_ExceptionMessageContainsRentalCount()
        {
            var firstRental = new Rental(
                1,
                new Game { Id = SampleGameIdentifier },
                new Account { Id = Guid.NewGuid(), DisplayName = "Renter" },
                new Account { Id = this.sampleOwnerIdentifier, DisplayName = "Owner" },
                DateTime.Now.AddDays(-1),
                DateTime.Now.AddDays(3));
            var secondRental = new Rental(
                2,
                new Game { Id = SampleGameIdentifier },
                new Account { Id = Guid.NewGuid(), DisplayName = "Renter" },
                new Account { Id = this.sampleOwnerIdentifier, DisplayName = "Owner" },
                DateTime.Now.AddDays(4),
                DateTime.Now.AddDays(6));

            this.rentalRepository.RentalsByGame = ImmutableList.Create(firstRental, secondRental);

            Action deleteAction = () => this.service.DeleteGameByIdentifier(SampleGameIdentifier);

            var exception = Assert.Throws<InvalidOperationException>(() => deleteAction());
            Assert.That(exception!.Message, Does.Contain("2 active rentals"));
        }

        [Test]
        public void GetGameByIdentifier_WithValidId_ReturnsGameDto()
        {
            this.gameRepository.GamesById[SampleGameIdentifier] = new Game { Id = SampleGameIdentifier };

            var retrievedGame = this.service.GetGameByIdentifier(SampleGameIdentifier);

            Assert.That(retrievedGame.Id, Is.EqualTo(SampleGameIdentifier));
        }

        [Test]
        public void UpdateGameByIdentifier_WithValidDto_CallsRepositoryUpdateWithCorrectId()
        {
            var gameDto = new GameDTO
            {
                Id = SampleGameIdentifier,
                Name = "Updated Game",
                Price = 12m,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
                Description = "A valid updated description for the game.",
                Owner = new UserDTO { Id = this.sampleOwnerIdentifier, DisplayName = "Owner" },
            };

            this.service.UpdateGameByIdentifier(SampleGameIdentifier, gameDto);

            Assert.That(this.gameRepository.UpdateCallCount, Is.EqualTo(1));
            Assert.That(this.gameRepository.LastUpdatedGameId, Is.EqualTo(SampleGameIdentifier));
        }

        [Test]
        public void DeleteGameByIdentifier_WithNoActiveRentals_DeletesGameAndNotifiesRequestService()
        {
            this.rentalRepository.RentalsByGame = ImmutableList<Rental>.Empty;
            this.gameRepository.GamesById[SampleGameIdentifier] = new Game { Id = SampleGameIdentifier };

            this.service.DeleteGameByIdentifier(SampleGameIdentifier);

            Assert.That(this.requestService.OnGameDeactivatedCallCount, Is.EqualTo(1));
            Assert.That(this.requestService.LastDeactivatedGameId, Is.EqualTo(SampleGameIdentifier));
            Assert.That(this.gameRepository.DeleteCallCount, Is.EqualTo(1));
            Assert.That(this.gameRepository.LastDeletedGameId, Is.EqualTo(SampleGameIdentifier));
        }
    }
}
