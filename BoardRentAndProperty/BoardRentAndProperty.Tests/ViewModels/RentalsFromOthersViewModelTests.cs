using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class RentalsFromOthersViewModelTests
    {
        private readonly Guid sampleRenterIdentifier = Guid.NewGuid();
        private Mock<IRentalService> rentalServiceMock = null!;
        private Mock<ICurrentUserContext> currentUserContextMock = null!;

        [SetUp]
        public void SetUp()
        {
            this.rentalServiceMock = new Mock<IRentalService>();
            this.currentUserContextMock = new Mock<ICurrentUserContext>();
            this.currentUserContextMock
                .SetupGet(context => context.CurrentUserId)
                .Returns(this.sampleRenterIdentifier);
            this.rentalServiceMock
                .Setup(service => service.GetRentalsForRenter(this.sampleRenterIdentifier))
                .Returns(ImmutableList<RentalDTO>.Empty);
        }

        [Test]
        public void Constructor_LoadsRentalsForCurrentRenter()
        {
            this.rentalServiceMock
                .Setup(service => service.GetRentalsForRenter(this.sampleRenterIdentifier))
                .Returns(ImmutableList.Create(BuildRental(1), BuildRental(2)));

            var viewModel = new RentalsFromOthersViewModel(this.rentalServiceMock.Object, this.currentUserContextMock.Object);

            viewModel.TotalCount.Should().Be(2);
        }

        [Test]
        public void Reload_OrdersRentalsByStartDateDescending()
        {
            var olderRental = BuildRental(1, DateTime.UtcNow.AddDays(2));
            var newerRental = BuildRental(2, DateTime.UtcNow.AddDays(10));
            this.rentalServiceMock
                .Setup(service => service.GetRentalsForRenter(this.sampleRenterIdentifier))
                .Returns(ImmutableList.Create(olderRental, newerRental));

            var viewModel = new RentalsFromOthersViewModel(this.rentalServiceMock.Object, this.currentUserContextMock.Object);

            viewModel.PagedItems[0].Id.Should().Be(2);
        }

        private RentalDTO BuildRental(int identifier, DateTime? startDate = null)
        {
            return new RentalDTO
            {
                Id = identifier,
                Game = new GameDTO { Id = 100 },
                Renter = new UserDTO { Id = this.sampleRenterIdentifier },
                Owner = new UserDTO { Id = Guid.NewGuid() },
                StartDate = startDate ?? DateTime.UtcNow.AddDays(1),
                EndDate = (startDate ?? DateTime.UtcNow.AddDays(1)).AddDays(2),
            };
        }
    }
}
