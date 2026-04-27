using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Services;

namespace BoardRentAndProperty.ViewModels
{
    public class RentalsToOthersViewModel : PagedViewModel<RentalDTO>
    {
        private readonly IRentalService rentalLookupService;
        private readonly ICurrentUserContext currentUserContext;

        public int CurrentGameOwnerUserId { get; private set; }

        public RentalsToOthersViewModel(IRentalService rentalLookupService, ICurrentUserContext currentUserContext)
        {
            this.rentalLookupService = rentalLookupService;
            this.currentUserContext = currentUserContext;
            Reload();
        }

        public override string ShowingText => $"Showing {DisplayedCount} of {TotalCount} rentals";

        public void LoadRentals() => Reload();

        protected override void Reload()
        {
            CurrentGameOwnerUserId = currentUserContext.CurrentUserId;
            var ownerRentalsSortedByNewest = rentalLookupService
                .GetRentalsForOwner(CurrentGameOwnerUserId)
                .OrderByDescending(rental => rental.StartDate)
                .ToImmutableList();
            SetAllItems(ownerRentalsSortedByNewest);
        }
    }
}