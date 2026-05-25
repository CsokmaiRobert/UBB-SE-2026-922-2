using System;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Contracts.Models;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Contracts
{
    [TestFixture]
    public sealed class RequestDtoTests
    {
        [Test]
        public void Status_DefaultsToOpen()
        {
            var requestDto = new RequestDTO();

            Assert.That(requestDto.Status, Is.EqualTo(RequestStatus.Open));
        }

        [Test]
        public void CanOffer_WhenStatusIsOpen_ReturnsTrue()
        {
            var requestDto = new RequestDTO { Status = RequestStatus.Open };

            Assert.That(requestDto.CanOffer, Is.True);
        }

        [Test]
        public void CanOffer_WhenStatusIsNotOpen_ReturnsFalse()
        {
            var requestDto = new RequestDTO { Status = RequestStatus.Accepted };

            Assert.That(requestDto.CanOffer, Is.False);
        }

        [Test]
        public void DateDisplayLongValues_IncludeStartAndEndPrefixes()
        {
            var requestDto = new RequestDTO
            {
                StartDate = new DateTime(2031, 3, 18),
                EndDate = new DateTime(2031, 8, 2),
            };

            Assert.That(requestDto.StartDateDisplayLong, Does.StartWith("Start: "));
            Assert.That(requestDto.EndDateDisplayLong, Does.StartWith("End: "));
        }

        [Test]
        public void PropertyAssignment_PreservesAllNavigationValues()
        {
            var requestDto = new RequestDTO
            {
                Id = 9,
                Game = new GameDTO { Id = 5 },
                Renter = new UserDTO { Id = Guid.NewGuid(), DisplayName = "Renter" },
                Owner = new UserDTO { Id = Guid.NewGuid(), DisplayName = "Owner" },
                OfferingUser = new UserDTO { Id = Guid.NewGuid(), DisplayName = "Bidder" },
            };

            Assert.That(requestDto.Id, Is.EqualTo(9));
            Assert.That(requestDto.Game!.Id, Is.EqualTo(5));
            Assert.That(requestDto.OfferingUser!.DisplayName, Is.EqualTo("Bidder"));
        }
    }
}
