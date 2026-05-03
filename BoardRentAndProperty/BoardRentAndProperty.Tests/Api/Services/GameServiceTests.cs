using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Repositories;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Services
{
    [TestFixture]
    public sealed class GameServiceTests
    {
        private const int SampleGameIdentifier = 42;

        private readonly Guid sampleOwnerIdentifier = Guid.NewGuid();
        private Mock<IGameRepository> gameRepositoryMock = null!;
        private Mock<IRentalRepository> rentalRepositoryMock = null!;
        private Mock<IRequestService> requestServiceMock = null!;
        private GameService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.gameRepositoryMock = new Mock<IGameRepository>();
            this.rentalRepositoryMock = new Mock<IRentalRepository>();
            this.requestServiceMock = new Mock<IRequestService>();

            this.rentalRepositoryMock
                .Setup(repository => repository.GetRentalsByGame(It.IsAny<int>()))
                .Returns(ImmutableList<Rental>.Empty);

            this.service = new GameService(
                this.gameRepositoryMock.Object,
                this.rentalRepositoryMock.Object,
                new GameMapper(new UserMapper()),
                this.requestServiceMock.Object);
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

            this.rentalRepositoryMock
                .Setup(repository => repository.GetRentalsByGame(SampleGameIdentifier))
                .Returns(ImmutableList.Create(activeRental));

            Action deleteAction = () => this.service.DeleteGameByIdentifier(SampleGameIdentifier);

            deleteAction.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*1 active rental*");
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

            this.gameRepositoryMock.Verify(repository => repository.Add(It.IsAny<Game>()), Times.Once);
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

            addAction.Should().Throw<ArgumentException>();
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

            this.rentalRepositoryMock
                .Setup(repository => repository.GetRentalsByGame(SampleGameIdentifier))
                .Returns(ImmutableList.Create(firstRental, secondRental));

            Action deleteAction = () => this.service.DeleteGameByIdentifier(SampleGameIdentifier);

            deleteAction.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*2 active rentals*");
        }

        [Test]
        public void GetGameByIdentifier_WithValidId_ReturnsGameDto()
        {
            this.gameRepositoryMock
                .Setup(repository => repository.Get(SampleGameIdentifier))
                .Returns(new Game { Id = SampleGameIdentifier });

            var retrievedGame = this.service.GetGameByIdentifier(SampleGameIdentifier);

            retrievedGame.Id.Should().Be(SampleGameIdentifier);
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

            this.gameRepositoryMock.Verify(
                repository => repository.Update(SampleGameIdentifier, It.IsAny<Game>()),
                Times.Once);
        }

        [Test]
        public void DeleteGameByIdentifier_WithNoActiveRentals_DeletesGameAndNotifiesRequestService()
        {
            this.rentalRepositoryMock
                .Setup(repository => repository.GetRentalsByGame(SampleGameIdentifier))
                .Returns(ImmutableList<Rental>.Empty);
            this.gameRepositoryMock
                .Setup(repository => repository.Delete(SampleGameIdentifier))
                .Returns(new Game { Id = SampleGameIdentifier });

            this.service.DeleteGameByIdentifier(SampleGameIdentifier);

            this.requestServiceMock.Verify(requestService => requestService.OnGameDeactivated(SampleGameIdentifier), Times.Once);
            this.gameRepositoryMock.Verify(repository => repository.Delete(SampleGameIdentifier), Times.Once);
        }
    }
}
