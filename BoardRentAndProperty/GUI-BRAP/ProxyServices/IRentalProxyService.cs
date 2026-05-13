using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace GUI_BRAP.ProxyServices
{
    public interface IRentalProxyService
    {
        Task<IReadOnlyList<RentalDTO>> GetRentalsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RentalDTO>> GetRentalsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default);
    }
}
