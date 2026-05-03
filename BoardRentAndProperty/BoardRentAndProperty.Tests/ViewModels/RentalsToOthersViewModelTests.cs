using System;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class RentalsToOthersViewModelTests
    {
        private readonly Guid ownerIdentifier = Guid.NewGuid();
        private readonly Guid renterIdentifier = Guid.NewGuid();
        private Mock<IRentalService> rentalServiceMock = null!;
        private Mock<ICurrentUserContext> currentUserContextMock = null!;

        [SetUp]
        public void SetUp()
        {
            this.rentalServiceMock = new Mock<IRentalService>();
            this.currentUserContextMock = new Mock<ICurrentUserContext>();
            this.currentUserContextMock.SetupGet(context => context.CurrentUserId).Returns(this.ownerIdentifier);
            this.rentalServiceMock
                .Setup(service => service.GetRentalsForOwner(this.ownerIdentifier))
                .Returns(ImmutableList<RentalDTO>.Empty);
        }

        [Test]
        public void ShowingText_WithRentals_ContainsCountAndRentalsKeyword()
        {
            this.rentalServiceMock
                .Setup(service => service.GetRentalsForOwner(this.ownerIdentifier))
                .Returns(ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30), BuildRental(50)));

            var viewModel = new RentalsToOthersViewModel(this.rentalServiceMock.Object, this.currentUserContextMock.Object);

            Assert.That(viewModel.ShowingText, Does.Contain("rentals"));
            Assert.That(viewModel.ShowingText, Does.Contain("4"));
        }

        [Test]
        public void Constructor_WithRentals_SetsCorrectOwnerIdAndTotalCount()
        {
            this.rentalServiceMock
                .Setup(service => service.GetRentalsForOwner(this.ownerIdentifier))
                .Returns(ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30), BuildRental(40)));

            var viewModel = new RentalsToOthersViewModel(this.rentalServiceMock.Object, this.currentUserContextMock.Object);

            Assert.That(viewModel.TotalCount, Is.EqualTo(4));
            Assert.That(viewModel.CurrentGameOwnerUserId, Is.EqualTo(this.ownerIdentifier));
        }

        [Test]
        public void Constructor_WithRentals_PagedItemsContainCorrectRentalDetails()
        {
            this.rentalServiceMock
                .Setup(service => service.GetRentalsForOwner(this.ownerIdentifier))
                .Returns(ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30)));

            var viewModel = new RentalsToOthersViewModel(this.rentalServiceMock.Object, this.currentUserContextMock.Object);
            var pagedRentalIds = viewModel.PagedItems.Select(rental => rental.Id).ToList();

            Assert.That(viewModel.PagedItems.All(rental => rental.Game.Id == 1), Is.True);
            Assert.That(viewModel.PagedItems.All(rental => rental.Owner.Id == this.ownerIdentifier), Is.True);
            Assert.That(pagedRentalIds, Does.Contain(10));
            Assert.That(pagedRentalIds, Does.Contain(20));
            Assert.That(pagedRentalIds, Does.Contain(30));
        }

        [Test]
        public void LoadRentals_AfterServiceDataChanged_RefreshesTotalCountAndPagedItems()
        {
            this.rentalServiceMock
                .Setup(service => service.GetRentalsForOwner(this.ownerIdentifier))
                .Returns(ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30)));

            var viewModel = new RentalsToOthersViewModel(this.rentalServiceMock.Object, this.currentUserContextMock.Object);
            Assert.That(viewModel.TotalCount, Is.EqualTo(3));

            this.rentalServiceMock
                .Setup(service => service.GetRentalsForOwner(this.ownerIdentifier))
                .Returns(ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30), BuildRental(50)));

            viewModel.LoadRentals();

            var pagedRentalIds = viewModel.PagedItems.Select(rental => rental.Id).ToList();

            Assert.That(viewModel.TotalCount, Is.EqualTo(4));
            Assert.That(pagedRentalIds, Does.Contain(50));
        }

        private RentalDTO BuildRental(int rentalId)
        {
            return new RentalDTO
            {
                Id = rentalId,
                Game = new GameDTO { Id = 1 },
                Renter = new UserDTO { Id = this.renterIdentifier },
                Owner = new UserDTO { Id = this.ownerIdentifier },
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
            };
        }
    }
}
