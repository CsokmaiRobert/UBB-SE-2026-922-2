using System;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class RentalsToOthersViewModelTests
    {
        private readonly Guid ownerIdentifier = Guid.NewGuid();
        private readonly Guid renterIdentifier = Guid.NewGuid();
        private FakeClientRentalService rentalService = null!;
        private FakeCurrentUserContext currentUserContext = null!;

        [SetUp]
        public void SetUp()
        {
            this.rentalService = new FakeClientRentalService();
            this.currentUserContext = new FakeCurrentUserContext { CurrentUserId = this.ownerIdentifier };
        }

        [Test]
        public void ShowingText_WithRentals_ContainsCountAndRentalsKeyword()
        {
            this.rentalService.RentalsForOwner =
                ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30), BuildRental(50));

            var viewModel = new RentalsToOthersViewModel(this.rentalService, this.currentUserContext);

            Assert.That(viewModel.ShowingText, Does.Contain("rentals"));
            Assert.That(viewModel.ShowingText, Does.Contain("4"));
        }

        [Test]
        public void Constructor_WithRentals_SetsCorrectOwnerIdAndTotalCount()
        {
            this.rentalService.RentalsForOwner =
                ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30), BuildRental(40));

            var viewModel = new RentalsToOthersViewModel(this.rentalService, this.currentUserContext);

            Assert.That(viewModel.TotalCount, Is.EqualTo(4));
            Assert.That(viewModel.CurrentGameOwnerUserId, Is.EqualTo(this.ownerIdentifier));
        }

        [Test]
        public void Constructor_WithRentals_PagedItemsContainCorrectRentalDetails()
        {
            this.rentalService.RentalsForOwner = ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30));

            var viewModel = new RentalsToOthersViewModel(this.rentalService, this.currentUserContext);
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
            this.rentalService.RentalsForOwner = ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30));

            var viewModel = new RentalsToOthersViewModel(this.rentalService, this.currentUserContext);
            Assert.That(viewModel.TotalCount, Is.EqualTo(3));

            this.rentalService.RentalsForOwner =
                ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30), BuildRental(50));

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
