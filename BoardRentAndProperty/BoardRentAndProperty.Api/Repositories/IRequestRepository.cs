using System;
using System.Collections.Immutable;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Contracts.Models;

namespace BoardRentAndProperty.Api.Repositories
{
    public interface IRequestRepository
    {
        ImmutableList<Request> GetAll();
        void Add(Request request);
        Request Delete(int id);
        void Update(int id, Request updated);
        Request Get(int id);
        void UpdateStatus(int requestId, RequestStatus status, Guid? offeringAccountId);
        ImmutableList<Request> GetRequestsByOwner(Guid ownerAccountId);
        ImmutableList<Request> GetRequestsByRenter(Guid renterAccountId);
        ImmutableList<Request> GetRequestsByGame(int gameId);
        ImmutableList<Request> GetOverlappingRequests(int gameId,
            int excludeRequestId,
            DateTime bufferedStartDate,
            DateTime bufferedEndDate);
        int ApproveAtomically(Request approvedRequest, ImmutableList<Request> overlappingRequests);
    }
}
