using System;
using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.ViewModels
{
    public class RentalsFromOthersViewModel : PagedViewModel<RentalDTO>
    {
        private readonly IRentalService rentalLookupService;
        private readonly ICurrentUserContext currentUserContext;

        public Guid CurrentRenterUserId { get; private set; }

        public RentalsFromOthersViewModel(IRentalService rentalLookupService, ICurrentUserContext currentUserContext)
        {
            this.rentalLookupService = rentalLookupService;
            this.currentUserContext = currentUserContext;
            Reload();
        }

        public override string ShowingText => $"Showing {DisplayedCount} of {TotalCount} rentals";

        public void LoadRentals() => Reload();

        protected override void Reload()
        {
            CurrentRenterUserId = currentUserContext.CurrentUserId;
            var currentUserRentalsSortedByNewest = rentalLookupService
                .GetRentalsForRenter(CurrentRenterUserId)
                .OrderByDescending(rental => rental.StartDate)
                .ToImmutableList();
            SetAllItems(currentUserRentalsSortedByNewest);
        }
    }
}