using System.Collections.Immutable;
using System.Linq;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.ViewModels
{
    public class RequestsFromOthersViewModel : PagedViewModel<RequestDTO>
    {
        private readonly IRequestService requestService;
        private readonly ICurrentUserContext currentUserContext;

        public int CurrentGameOwnerUserId { get; private set; }

        public RequestsFromOthersViewModel(IRequestService requestService, ICurrentUserContext currentUserContext)
        {
            this.requestService = requestService;
            this.currentUserContext = currentUserContext;
            Reload();
        }

        public override string ShowingText => $"Showing {DisplayedCount} of {TotalCount} requests";

        public void LoadRequests() => Reload();

        protected override void Reload()
        {
            CurrentGameOwnerUserId = currentUserContext.CurrentUserId;

            var openRequestsForOwnerSortedByNewest = requestService
                .GetOpenRequestsForOwner(CurrentGameOwnerUserId)
                .OrderByDescending(request => request.StartDate)
                .ToImmutableList();
            SetAllItems(openRequestsForOwnerSortedByNewest);
        }

        public string? TryApproveRequest(int requestIdToApprove)
        {
            var approvalResult = requestService.ApproveRequest(requestIdToApprove, CurrentGameOwnerUserId);
            if (approvalResult.IsSuccess)
            {
                Reload();
                return null;
            }

            return approvalResult.Error switch
            {
                ApproveRequestError.Unauthorized => "You are not authorized to approve this request.",
                ApproveRequestError.NotFound => "Request not found.",
                ApproveRequestError.TransactionFailed => "Could not approve the request. Please try again.",
                _ => Constants.DialogMessages.UnexpectedErrorOccurred
            };
        }

        public string? TryDenyRequest(int requestIdToDeny, string? rawDenialReason)
        {
            var denialResult = requestService.DenyRequest(requestIdToDeny, CurrentGameOwnerUserId, rawDenialReason ?? string.Empty);
            if (denialResult.IsSuccess)
            {
                Reload();
                return null;
            }

            return denialResult.Error switch
            {
                DenyRequestError.NotFound => "Request not found.",
                DenyRequestError.Unauthorized => "You are not authorized to deny this request.",
                _ => Constants.DialogMessages.UnexpectedErrorOccurred
            };
        }

        public string? TryOfferGame(int requestIdForGameOffer)
        {
            var gameOfferResult = requestService.OfferGame(requestIdForGameOffer, CurrentGameOwnerUserId);
            if (gameOfferResult.IsSuccess)
            {
                Reload();
                return null;
            }

            return gameOfferResult.Error switch
            {
                OfferError.NotFound => "Request not found.",
                OfferError.NotOwner => "You are not the owner of this game.",
                OfferError.RequestNotOpen => "This request is no longer open.",
                OfferError.TransactionFailed => "Could not approve the request. Please try again.",
                _ => Constants.DialogMessages.UnexpectedErrorOccurred
            };
        }
    }
}
