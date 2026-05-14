using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace GUI_BRAP.ProxyServices
{
    public interface IRequestProxyService
    {
        Task<IReadOnlyList<RequestDTO>> GetOpenRequestsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default);

        Task OfferGameAsync(int requestId, RequestActionDataTransferObject body, CancellationToken cancellationToken = default);

        Task DenyRequestAsync(int requestId, RequestActionDataTransferObject body, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<RequestDTO>> GetRequestsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default);

        Task CreateRequestAsync(CreateRequestDataTransferObject body, CancellationToken cancellationToken = default);

        Task CancelRequestAsync(int requestId, RequestActionDataTransferObject body, CancellationToken cancellationToken = default);
    }
}
