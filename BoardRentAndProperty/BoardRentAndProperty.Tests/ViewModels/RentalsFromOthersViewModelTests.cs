using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class RentalsFromOthersViewModelTests
    {
        private readonly Guid sampleRenterIdentifier = Guid.NewGuid();
        private FakeClientRentalService rentalService = null!;
        private FakeCurrentUserContext currentUserContext = null!;

        [SetUp]
        public void SetUp()
        {
            this.rentalService = new FakeClientRentalService();
            this.currentUserContext = new FakeCurrentUserContext { CurrentUserId = this.sampleRenterIdentifier };
        }

        [Test]
        public void Constructor_LoadsRentalsForCurrentRenter()
        {
            this.rentalService.RentalsForRenter = ImmutableList.Create(BuildRental(1), BuildRental(2));

            var viewModel = new RentalsFromOthersViewModel(this.rentalService, this.currentUserContext);

            Assert.That(viewModel.TotalCount, Is.EqualTo(2));
        }

        [Test]
        public void Reload_OrdersRentalsByStartDateDescending()
        {
            var olderRental = BuildRental(1, DateTime.UtcNow.AddDays(2));
            var newerRental = BuildRental(2, DateTime.UtcNow.AddDays(10));
            this.rentalService.RentalsForRenter = ImmutableList.Create(olderRental, newerRental);

            var viewModel = new RentalsFromOthersViewModel(this.rentalService, this.currentUserContext);

            Assert.That(viewModel.PagedItems[0].Id, Is.EqualTo(2));
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
