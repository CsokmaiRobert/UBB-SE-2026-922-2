using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.ProxyServices;

namespace GUI_BRAP.Infrastructure
{
    public sealed class RentalProxyServiceAdapter : IRentalProxyService
    {
        private readonly IRentalService rentalService;

        public RentalProxyServiceAdapter(IRentalService rentalService)
        {
            this.rentalService = rentalService;
        }

        public async Task<IReadOnlyList<RentalDTO>> GetRentalsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
            => (await this.rentalService.GetRentalsForOwnerAsync(ownerAccountId, cancellationToken)).ThrowIfFailed();

        public async Task<IReadOnlyList<RentalDTO>> GetRentalsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
            => (await this.rentalService.GetRentalsForRenterAsync(renterAccountId, cancellationToken)).ThrowIfFailed();
    }
}
