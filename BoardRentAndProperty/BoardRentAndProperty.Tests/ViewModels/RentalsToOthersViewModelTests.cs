using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
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
        public void Constructor_WithRentals_ExposesOwnerIdAndPopulatesPagedItems()
        {
            this.rentalService.RentalsForOwner = ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30));

            var viewModel = new RentalsToOthersViewModel(this.rentalService, this.currentUserContext);

            Assert.That(viewModel.TotalCount, Is.EqualTo(3));
            Assert.That(viewModel.CurrentGameOwnerUserId, Is.EqualTo(this.ownerIdentifier));
            Assert.That(viewModel.PagedItems.All(rental => rental.Owner.Id == this.ownerIdentifier), Is.True);
            Assert.That(viewModel.ShowingText, Does.Contain("rentals"));
        }

        [Test]
        public async Task LoadRentals_AfterServiceDataChanged_RefreshesTotalCountAndPagedItems()
        {
            this.rentalService.RentalsForOwner = ImmutableList.Create(BuildRental(10), BuildRental(20));
            var viewModel = new RentalsToOthersViewModel(this.rentalService, this.currentUserContext);
            Assert.That(viewModel.TotalCount, Is.EqualTo(2));

            this.rentalService.RentalsForOwner = ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(50));
            await viewModel.LoadRentalsAsync();

            Assert.That(viewModel.TotalCount, Is.EqualTo(3));
            Assert.That(viewModel.PagedItems.Select(rental => rental.Id), Does.Contain(50));
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
