using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Tests.Fakes;
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
        private FakeRentalRepository rentalRepository = null!;
        private FakeGameRepository gameRepository = null!;
        private RentalService service = null!;

        [SetUp]
        public void SetUp()
        {
            this.rentalRepository = new FakeRentalRepository();
            this.gameRepository = new FakeGameRepository();
            this.gameRepository.GamesById[ActiveGameId] = new Game
                {
                    Id = ActiveGameId,
                    Owner = new Account { Id = this.ownerId, DisplayName = "Owner" },
                    IsActive = true,
                };
            this.gameRepository.GamesById[SecondGameId] = new Game
                {
                    Id = SecondGameId,
                    Owner = new Account { Id = this.ownerId, DisplayName = "Owner" },
                    IsActive = false,
                };

            this.service = new RentalService(
                this.rentalRepository,
                this.gameRepository,
                new RentalMapper(new GameMapper(new UserMapper()), new UserMapper()));
        }

        [Test]
        public void CreateConfirmedRental_WithCorrectOwner_CallsAddConfirmedForEachGame()
        {
            this.service.CreateConfirmedRental(SecondGameId, this.renterId, this.ownerId, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(3));
            Assert.That(this.rentalRepository.AddConfirmedCallCount, Is.EqualTo(1));

            this.service.CreateConfirmedRental(ActiveGameId, this.renterId, this.ownerId, DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(6));
            Assert.That(this.rentalRepository.AddConfirmedCallCount, Is.EqualTo(2));
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

            this.rentalRepository.RentalsByGameId[ActiveGameId] = ImmutableList.Create(existingRental);
            this.rentalRepository.RentalsByGameId[SecondGameId] = ImmutableList<Rental>.Empty;

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

            this.rentalRepository.RentalsByGameId[ActiveGameId] = ImmutableList.Create(existingRental);
            this.rentalRepository.RentalsByGameId[SecondGameId] = ImmutableList<Rental>.Empty;

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
