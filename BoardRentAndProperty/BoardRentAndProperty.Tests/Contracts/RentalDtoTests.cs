using System;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Contracts
{
    [TestFixture]
    public sealed class RentalDtoTests
    {
        [Test]
        public void IsExpired_WhenEndDateIsInThePast_ReturnsTrue()
        {
            var rentalDto = new RentalDTO { EndDate = DateTime.UtcNow.AddDays(-1) };

            Assert.That(rentalDto.IsExpired, Is.True);
        }

        [Test]
        public void IsExpired_WhenEndDateIsInTheFuture_ReturnsFalse()
        {
            var rentalDto = new RentalDTO { EndDate = DateTime.UtcNow.AddDays(7) };

            Assert.That(rentalDto.IsExpired, Is.False);
        }

        [Test]
        public void StartDateDisplayLong_ContainsStartPrefix()
        {
            var rentalDto = new RentalDTO { StartDate = new DateTime(2030, 1, 15) };

            Assert.That(rentalDto.StartDateDisplayLong, Does.StartWith("Start: "));
        }

        [Test]
        public void EndDateDisplayLong_ContainsEndPrefix()
        {
            var rentalDto = new RentalDTO { EndDate = new DateTime(2030, 6, 30) };

            Assert.That(rentalDto.EndDateDisplayLong, Does.StartWith("End: "));
        }

        [Test]
        public void PropertyAssignment_PreservesValues()
        {
            var renter = new UserDTO { Id = Guid.NewGuid(), DisplayName = "Alice" };
            var owner = new UserDTO { Id = Guid.NewGuid(), DisplayName = "Bob" };
            var game = new GameDTO { Id = 1, Name = "Risk" };

            var rentalDto = new RentalDTO
            {
                Id = 42,
                Game = game,
                Renter = renter,
                Owner = owner,
                StartDate = new DateTime(2030, 1, 1),
                EndDate = new DateTime(2030, 1, 5),
            };

            Assert.That(rentalDto.Id, Is.EqualTo(42));
            Assert.That(rentalDto.Game, Is.SameAs(game));
            Assert.That(rentalDto.Renter, Is.SameAs(renter));
            Assert.That(rentalDto.Owner, Is.SameAs(owner));
        }
    }
}
