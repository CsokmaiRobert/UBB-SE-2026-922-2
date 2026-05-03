using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Repositories;
using BoardRentAndProperty.Api.Services;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Services
{
    [TestFixture]
    public sealed class ServiceRentalTests
    {
        private const int ActiveGameId = 10;
        private const int SecondGameId = 20;

        private readonly Guid ownerId = Guid.NewGuid();
        private readonly Guid renterId = Guid.NewGuid();
        private readonly Guid fakeOwnerId = Guid.NewGuid();
        private Mock<IRentalRepository> rentalRepositoryMock = null!;
        private Mock<IGameRepository> gameRepositoryMock = null!;
        private RentalService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.rentalRepositoryMock = new Mock<IRentalRepository>();
            this.gameRepositoryMock = new Mock<IGameRepository>();

            this.gameRepositoryMock
                .Setup(repository => repository.Get(ActiveGameId))
                .Returns(new Game
                {
                    Id = ActiveGameId,
                    Owner = new Account { Id = this.ownerId, DisplayName = "Owner" },
                    IsActive = true,
                });
            this.gameRepositoryMock
                .Setup(repository => repository.Get(SecondGameId))
                .Returns(new Game
                {
                    Id = SecondGameId,
                    Owner = new Account { Id = this.ownerId, DisplayName = "Owner" },
                    IsActive = false,
                });

            this.rentalRepositoryMock
                .Setup(repository => repository.GetRentalsByGame(ActiveGameId))
                .Returns(ImmutableList<Rental>.Empty);
            this.rentalRepositoryMock
                .Setup(repository => repository.GetRentalsByGame(SecondGameId))
                .Returns(ImmutableList<Rental>.Empty);

            this.service = new RentalService(
                this.rentalRepositoryMock.Object,
                this.gameRepositoryMock.Object,
                new RentalMapper(new GameMapper(new UserMapper()), new UserMapper()));
        }

        [Test]
        public void CreateConfirmedRental_WithCorrectOwner_CallsAddConfirmedForEachGame()
        {
            this.service.CreateConfirmedRental(SecondGameId, this.renterId, this.ownerId, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(3));
            this.rentalRepositoryMock.Verify(repository => repository.AddConfirmed(It.IsAny<Rental>()), Times.Once);

            this.service.CreateConfirmedRental(ActiveGameId, this.renterId, this.ownerId, DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(6));
            this.rentalRepositoryMock.Verify(repository => repository.AddConfirmed(It.IsAny<Rental>()), Times.Exactly(2));
        }

        [Test]
        public void CreateConfirmedRental_WithWrongOwnerId_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                this.service.CreateConfirmedRental(
                    ActiveGameId,
                    this.renterId,
                    this.fakeOwnerId,
                    DateTime.UtcNow.AddDays(1),
                    DateTime.UtcNow.AddDays(3)));
        }

        [Test]
        public void CreateRental_WithInvalidDateRange_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                this.service.CreateConfirmedRental(
                    ActiveGameId,
                    this.renterId,
                    this.ownerId,
                    DateTime.UtcNow.AddDays(4),
                    DateTime.UtcNow.AddDays(2)));
        }

        [Test]
        public void CreateConfirmedRental_OnOverlappingDates_ThrowsInvalidOperationExceptionOnlyForSameGame()
        {
            var existingRental = BuildRental(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(3));

            this.rentalRepositoryMock
                .Setup(repository => repository.GetRentalsByGame(ActiveGameId))
                .Returns(ImmutableList.Create(existingRental));

            Assert.Throws<InvalidOperationException>(() =>
                this.service.CreateConfirmedRental(
                    ActiveGameId,
                    this.renterId,
                    this.ownerId,
                    DateTime.UtcNow.AddDays(2),
                    DateTime.UtcNow.AddDays(3)));
            Assert.DoesNotThrow(() =>
                this.service.CreateConfirmedRental(
                    SecondGameId,
                    this.renterId,
                    this.ownerId,
                    DateTime.UtcNow.AddDays(2),
                    DateTime.UtcNow.AddDays(3)));
        }

        [Test]
        public void IsSlotAvailable_DuringBufferPeriod_ReturnsFalseOnlyForSameGame()
        {
            var existingRental = BuildRental(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2));

            this.rentalRepositoryMock
                .Setup(repository => repository.GetRentalsByGame(ActiveGameId))
                .Returns(ImmutableList.Create(existingRental));

            bool isAvailable = this.service.IsSlotAvailable(
                ActiveGameId,
                DateTime.UtcNow.AddDays(3),
                DateTime.UtcNow.AddDays(4));
            bool isAvailableForSecondGame = this.service.IsSlotAvailable(
                SecondGameId,
                DateTime.UtcNow.AddDays(3),
                DateTime.UtcNow.AddDays(4));

            Assert.That(isAvailable, Is.False);
            Assert.That(isAvailableForSecondGame, Is.True);
        }

        private Rental BuildRental(DateTime startDate, DateTime endDate)
        {
            return new Rental(
                1,
                new Game { Id = ActiveGameId },
                new Account { Id = this.renterId, DisplayName = "Renter" },
                new Account { Id = this.ownerId, DisplayName = "Owner" },
                startDate,
                endDate);
        }
    }
}
