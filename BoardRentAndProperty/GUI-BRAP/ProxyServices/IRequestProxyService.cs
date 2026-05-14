using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace GUI_BRAP.ProxyServices
{
    public interface IRequestProxyService
    {
        Task<IReadOnlyList<RequestDTO>> GetRequestsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default);

        Task CreateRequestAsync(CreateRequestDataTransferObject body, CancellationToken cancellationToken = default);

        Task CancelRequestAsync(int requestId, RequestActionDataTransferObject body, CancellationToken cancellationToken = default);
    }
}
