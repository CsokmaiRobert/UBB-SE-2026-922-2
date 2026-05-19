using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using GUI_BRAP.ProxyServices;

namespace GUI_BRAP.Infrastructure
{
    public sealed class RequestProxyServiceAdapter : IRequestProxyService
    {
        private readonly IRequestService requestService;

        public RequestProxyServiceAdapter(IRequestService requestService)
        {
            this.requestService = requestService;
        }

        public async Task<IReadOnlyList<RequestDTO>> GetOpenRequestsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default)
            => (await this.requestService.GetOpenRequestsForOwnerAsync(ownerAccountId, cancellationToken)).ThrowIfFailed();

        public async Task<IReadOnlyList<RequestDTO>> GetRequestsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default)
            => (await this.requestService.GetRequestsForRenterAsync(renterAccountId, cancellationToken)).ThrowIfFailed();

        public async Task CreateRequestAsync(CreateRequestDataTransferObject body, CancellationToken cancellationToken = default)
            => (await this.requestService.CreateRequestAsync(body, cancellationToken)).ThrowIfFailed();

        public async Task OfferGameAsync(int requestId, RequestActionDataTransferObject body, CancellationToken cancellationToken = default)
            => (await this.requestService.OfferGameAsync(requestId, body, cancellationToken)).ThrowIfFailed();

        public async Task DenyRequestAsync(int requestId, RequestActionDataTransferObject body, CancellationToken cancellationToken = default)
            => (await this.requestService.DenyRequestAsync(requestId, body, cancellationToken)).ThrowIfFailed();

        public async Task CancelRequestAsync(int requestId, RequestActionDataTransferObject body, CancellationToken cancellationToken = default)
            => (await this.requestService.CancelRequestAsync(requestId, body, cancellationToken)).ThrowIfFailed();
    }
}
