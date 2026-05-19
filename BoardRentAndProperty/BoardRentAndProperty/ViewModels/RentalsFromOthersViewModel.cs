using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
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
            _ = this.ReloadAsync();
        }

        public override string ShowingText => $"Showing {DisplayedCount} of {TotalCount} rentals";

        public Task LoadRentalsAsync() => this.ReloadAsync();

        protected override void Reload()
        {
            _ = this.ReloadAsync();
        }

        private async Task ReloadAsync()
        {
            CurrentRenterUserId = currentUserContext.CurrentUserId;
            var rentalsResult = await this.rentalLookupService.GetRentalsForRenterAsync(CurrentRenterUserId);
            var currentUserRentalsSortedByNewest = rentalsResult.Success && rentalsResult.Data != null
                ? rentalsResult.Data.OrderByDescending(rental => rental.StartDate).ToImmutableList()
                : ImmutableList<RentalDTO>.Empty;
            SetAllItems(currentUserRentalsSortedByNewest);
        }
    }
}
