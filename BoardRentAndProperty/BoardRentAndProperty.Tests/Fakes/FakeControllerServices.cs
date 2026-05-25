using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using ApiAuthService = BoardRentAndProperty.Api.Services.IAuthService;
using ApiAccountService = BoardRentAndProperty.Api.Services.IAccountService;
using ApiAdminService = BoardRentAndProperty.Api.Services.IAdminService;
using ApiGameService = BoardRentAndProperty.Api.Services.IGameService;
using ApiRentalService = BoardRentAndProperty.Api.Services.IRentalService;
using ApiUserService = BoardRentAndProperty.Api.Services.IUserService;
using ApiNotificationService = BoardRentAndProperty.Api.Services.INotificationService;
using ApiRequestService = BoardRentAndProperty.Api.Services.IRequestService;
using ApiBookedDateRange = BoardRentAndProperty.Api.Services.BookedDateRange;
using ApiCreateRequestError = BoardRentAndProperty.Api.Services.CreateRequestError;
using ApiApproveRequestError = BoardRentAndProperty.Api.Services.ApproveRequestError;
using ApiDenyRequestError = BoardRentAndProperty.Api.Services.DenyRequestError;
using ApiCancelRequestError = BoardRentAndProperty.Api.Services.CancelRequestError;
using ApiOfferError = BoardRentAndProperty.Api.Services.OfferError;
using ApiResult = BoardRentAndProperty.Api.Services.Result<int, BoardRentAndProperty.Api.Services.CreateRequestError>;
using ApiServiceResult = BoardRentAndProperty.Api.Utilities.ServiceResult<bool>;
using ApiProfileResult = BoardRentAndProperty.Api.Utilities.ServiceResult<BoardRentAndProperty.Contracts.DataTransferObjects.AccountProfileDataTransferObject>;
using ApiStringResult = BoardRentAndProperty.Api.Utilities.ServiceResult<string>;
using ApiAccountListResult = BoardRentAndProperty.Api.Utilities.ServiceResult<System.Collections.Generic.List<BoardRentAndProperty.Contracts.DataTransferObjects.AccountProfileDataTransferObject>>;

namespace BoardRentAndProperty.Tests.Fakes
{
    internal sealed class FakeAuthService : ApiAuthService
    {
        public ApiServiceResult RegisterOutcome { get; set; } = ApiServiceResult.Ok(true);
        public ApiProfileResult LoginOutcome { get; set; } = ApiProfileResult.Ok(new AccountProfileDataTransferObject());
        public ApiServiceResult LogoutOutcome { get; set; } = ApiServiceResult.Ok(true);
        public ApiStringResult ForgotPasswordOutcome { get; set; } = ApiStringResult.Ok("reset-token");
        public RegisterDataTransferObject? LastRegisterPayload { get; private set; }
        public LoginDataTransferObject? LastLoginPayload { get; private set; }

        public Task<ApiServiceResult> RegisterAsync(RegisterDataTransferObject dto)
        {
            this.LastRegisterPayload = dto;
            return Task.FromResult(this.RegisterOutcome);
        }

        public Task<ApiProfileResult> LoginAsync(LoginDataTransferObject dto)
        {
            this.LastLoginPayload = dto;
            return Task.FromResult(this.LoginOutcome);
        }

        public Task<ApiServiceResult> LogoutAsync() => Task.FromResult(this.LogoutOutcome);

        public Task<ApiStringResult> ForgotPasswordAsync() => Task.FromResult(this.ForgotPasswordOutcome);
    }

    internal sealed class FakeAccountService : ApiAccountService
    {
        public ApiProfileResult GetProfileOutcome { get; set; } = ApiProfileResult.Ok(new AccountProfileDataTransferObject());
        public ApiServiceResult UpdateProfileOutcome { get; set; } = ApiServiceResult.Ok(true);
        public ApiServiceResult ChangePasswordOutcome { get; set; } = ApiServiceResult.Ok(true);
        public ApiStringResult SetAvatarUrlOutcome { get; set; } = ApiStringResult.Ok("/avatars/x.png");
        public ApiServiceResult RemoveAvatarOutcome { get; set; } = ApiServiceResult.Ok(true);

        public Guid LastQueriedAccountId { get; private set; }
        public AccountProfileDataTransferObject? LastUpdateProfilePayload { get; private set; }
        public string? LastChangePasswordCurrent { get; private set; }
        public string? LastChangePasswordNew { get; private set; }
        public string? LastAvatarRelativeUrl { get; private set; }
        public Guid LastRemoveAvatarAccountId { get; private set; }

        public Task<ApiProfileResult> GetProfileAsync(Guid accountId)
        {
            this.LastQueriedAccountId = accountId;
            return Task.FromResult(this.GetProfileOutcome);
        }

        public Task<ApiServiceResult> UpdateProfileAsync(Guid accountId, AccountProfileDataTransferObject profileUpdateData)
        {
            this.LastQueriedAccountId = accountId;
            this.LastUpdateProfilePayload = profileUpdateData;
            return Task.FromResult(this.UpdateProfileOutcome);
        }

        public Task<ApiServiceResult> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword)
        {
            this.LastQueriedAccountId = accountId;
            this.LastChangePasswordCurrent = currentPassword;
            this.LastChangePasswordNew = newPassword;
            return Task.FromResult(this.ChangePasswordOutcome);
        }

        public Task<ApiStringResult> SetAvatarUrlAsync(Guid accountId, string avatarRelativeUrl)
        {
            this.LastQueriedAccountId = accountId;
            this.LastAvatarRelativeUrl = avatarRelativeUrl;
            return Task.FromResult(this.SetAvatarUrlOutcome);
        }

        public Task<ApiServiceResult> RemoveAvatarAsync(Guid accountId)
        {
            this.LastRemoveAvatarAccountId = accountId;
            return Task.FromResult(this.RemoveAvatarOutcome);
        }
    }

    internal sealed class FakeAdminService : ApiAdminService
    {
        public ApiAccountListResult GetAllOutcome { get; set; } = ApiAccountListResult.Ok(new List<AccountProfileDataTransferObject>());
        public ApiServiceResult SuspendOutcome { get; set; } = ApiServiceResult.Ok(true);
        public ApiServiceResult UnsuspendOutcome { get; set; } = ApiServiceResult.Ok(true);
        public ApiServiceResult ResetPasswordOutcome { get; set; } = ApiServiceResult.Ok(true);
        public ApiServiceResult UnlockOutcome { get; set; } = ApiServiceResult.Ok(true);

        public int LastPageNumber { get; private set; }
        public int LastPageSize { get; private set; }
        public Guid LastTargetAccountId { get; private set; }
        public string? LastResetPasswordValue { get; private set; }

        public Task<ApiAccountListResult> GetAllAccountsAsync(int page, int pageSize)
        {
            this.LastPageNumber = page;
            this.LastPageSize = pageSize;
            return Task.FromResult(this.GetAllOutcome);
        }

        public Task<ApiServiceResult> SuspendAccountAsync(Guid accountId)
        {
            this.LastTargetAccountId = accountId;
            return Task.FromResult(this.SuspendOutcome);
        }

        public Task<ApiServiceResult> UnsuspendAccountAsync(Guid accountId)
        {
            this.LastTargetAccountId = accountId;
            return Task.FromResult(this.UnsuspendOutcome);
        }

        public Task<ApiServiceResult> ResetPasswordAsync(Guid accountId, string newPassword)
        {
            this.LastTargetAccountId = accountId;
            this.LastResetPasswordValue = newPassword;
            return Task.FromResult(this.ResetPasswordOutcome);
        }

        public Task<ApiServiceResult> UnlockAccountAsync(Guid accountId)
        {
            this.LastTargetAccountId = accountId;
            return Task.FromResult(this.UnlockOutcome);
        }
    }

    internal sealed class FakeGameService : ApiGameService
    {
        public ImmutableList<GameDTO> AllGames { get; set; } = ImmutableList<GameDTO>.Empty;
        public ImmutableList<GameDTO> OwnerGames { get; set; } = ImmutableList<GameDTO>.Empty;
        public ImmutableList<GameDTO> ActiveOwnerGames { get; set; } = ImmutableList<GameDTO>.Empty;
        public ImmutableList<GameDTO> AvailableRenterGames { get; set; } = ImmutableList<GameDTO>.Empty;
        public Dictionary<int, GameDTO> GamesById { get; } = new Dictionary<int, GameDTO>();
        public GameDTO? DeletedGame { get; set; }
        public Exception? AddException { get; set; }
        public Exception? UpdateException { get; set; }
        public Exception? GetByIdentifierException { get; set; }
        public Exception? DeleteException { get; set; }

        public int AddCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }
        public int DeleteCallCount { get; private set; }
        public GameDTO? LastAddedGame { get; private set; }
        public int LastUpdatedGameId { get; private set; }
        public GameDTO? LastUpdatedGamePayload { get; private set; }
        public int LastDeletedGameId { get; private set; }

        public void AddGame(GameDTO gameDto)
        {
            if (this.AddException != null)
            {
                throw this.AddException;
            }

            this.AddCallCount++;
            this.LastAddedGame = gameDto;
        }

        public void UpdateGameByIdentifier(int gameId, GameDTO updatedGameDTO)
        {
            if (this.UpdateException != null)
            {
                throw this.UpdateException;
            }

            this.UpdateCallCount++;
            this.LastUpdatedGameId = gameId;
            this.LastUpdatedGamePayload = updatedGameDTO;
        }

        public GameDTO DeleteGameByIdentifier(int gameId)
        {
            if (this.DeleteException != null)
            {
                throw this.DeleteException;
            }

            this.DeleteCallCount++;
            this.LastDeletedGameId = gameId;
            return this.DeletedGame ?? new GameDTO { Id = gameId };
        }

        public GameDTO GetGameByIdentifier(int gameId)
        {
            if (this.GetByIdentifierException != null)
            {
                throw this.GetByIdentifierException;
            }

            return this.GamesById.TryGetValue(gameId, out var game) ? game : new GameDTO { Id = gameId };
        }

        public ImmutableList<GameDTO> GetGamesForOwner(Guid ownerAccountId) => this.OwnerGames;

        public ImmutableList<GameDTO> GetAllGames() => this.AllGames;

        public List<string> ValidateGame(GameDTO gameDto) => new List<string>();

        public ImmutableList<GameDTO> GetAvailableGamesForRenter(Guid renterAccountId) => this.AvailableRenterGames;

        public ImmutableList<GameDTO> GetActiveGamesForOwner(Guid ownerAccountId) => this.ActiveOwnerGames;
    }

    internal sealed class FakeRentalService : ApiRentalService
    {
        public ImmutableList<RentalDTO> RenterRentals { get; set; } = ImmutableList<RentalDTO>.Empty;
        public ImmutableList<RentalDTO> OwnerRentals { get; set; } = ImmutableList<RentalDTO>.Empty;
        public bool SlotAvailableOutcome { get; set; } = true;
        public Exception? CreateException { get; set; }

        public int CreateCallCount { get; private set; }
        public int LastCreatedGameId { get; private set; }
        public Guid LastCreatedRenterId { get; private set; }
        public Guid LastCreatedOwnerId { get; private set; }
        public DateTime LastCreatedStartDate { get; private set; }
        public DateTime LastCreatedEndDate { get; private set; }

        public ImmutableList<RentalDTO> GetRentalsForRenter(Guid renterAccountId) => this.RenterRentals;

        public ImmutableList<RentalDTO> GetRentalsForOwner(Guid ownerAccountId) => this.OwnerRentals;

        public bool IsSlotAvailable(int gameId, DateTime requestedStartDate, DateTime requestedEndDate) => this.SlotAvailableOutcome;

        public void CreateConfirmedRental(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate)
        {
            if (this.CreateException != null)
            {
                throw this.CreateException;
            }

            this.CreateCallCount++;
            this.LastCreatedGameId = gameId;
            this.LastCreatedRenterId = renterAccountId;
            this.LastCreatedOwnerId = ownerAccountId;
            this.LastCreatedStartDate = startDate;
            this.LastCreatedEndDate = endDate;
        }
    }

    internal sealed class FakeUserService : ApiUserService
    {
        public ImmutableList<UserDTO> AvailableUsers { get; set; } = ImmutableList<UserDTO>.Empty;
        public Guid LastExcludedAccountId { get; private set; }

        public ImmutableList<UserDTO> GetUsersExcept(Guid excludeAccountId)
        {
            this.LastExcludedAccountId = excludeAccountId;
            return this.AvailableUsers;
        }
    }

    internal sealed class FakeNotificationServiceForController : ApiNotificationService
    {
        public ImmutableList<NotificationDTO> UserNotifications { get; set; } = ImmutableList<NotificationDTO>.Empty;
        public Dictionary<int, NotificationDTO> NotificationsById { get; } = new Dictionary<int, NotificationDTO>();
        public Exception? GetByIdentifierException { get; set; }
        public Exception? UpdateException { get; set; }
        public Exception? DeleteException { get; set; }

        public int UpdateCallCount { get; private set; }
        public int DeleteCallCount { get; private set; }

        public ImmutableList<NotificationDTO> GetNotificationsForUser(Guid accountId) => this.UserNotifications;

        public NotificationDTO GetNotificationByIdentifier(int notificationId)
        {
            if (this.GetByIdentifierException != null)
            {
                throw this.GetByIdentifierException;
            }

            return this.NotificationsById.TryGetValue(notificationId, out var notification)
                ? notification
                : new NotificationDTO { Id = notificationId };
        }

        public NotificationDTO DeleteNotificationByIdentifier(int notificationId)
        {
            if (this.DeleteException != null)
            {
                throw this.DeleteException;
            }

            this.DeleteCallCount++;
            return this.NotificationsById.TryGetValue(notificationId, out var notification)
                ? notification
                : new NotificationDTO { Id = notificationId };
        }

        public void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedNotificationDto)
        {
            if (this.UpdateException != null)
            {
                throw this.UpdateException;
            }

            this.UpdateCallCount++;
        }

        public void SendNotificationToUser(Guid recipientAccountId, NotificationDTO notificationDto)
        {
        }

        public void DeleteNotificationsLinkedToRequest(int relatedRequestId)
        {
        }
    }

    internal sealed class ConfigurableFakeRequestService : ApiRequestService
    {
        public ImmutableList<RequestDTO> RenterRequests { get; set; } = ImmutableList<RequestDTO>.Empty;
        public ImmutableList<RequestDTO> OwnerRequests { get; set; } = ImmutableList<RequestDTO>.Empty;
        public ImmutableList<RequestDTO> OpenOwnerRequests { get; set; } = ImmutableList<RequestDTO>.Empty;
        public ImmutableList<ApiBookedDateRange> BookedDates { get; set; } = ImmutableList<ApiBookedDateRange>.Empty;
        public bool AvailabilityResult { get; set; } = true;

        public BoardRentAndProperty.Api.Services.Result<int, ApiCreateRequestError> CreateOutcome { get; set; } =
            BoardRentAndProperty.Api.Services.Result<int, ApiCreateRequestError>.Success(123);
        public BoardRentAndProperty.Api.Services.Result<int, ApiApproveRequestError> ApproveOutcome { get; set; } =
            BoardRentAndProperty.Api.Services.Result<int, ApiApproveRequestError>.Success(50);
        public BoardRentAndProperty.Api.Services.Result<int, ApiDenyRequestError> DenyOutcome { get; set; } =
            BoardRentAndProperty.Api.Services.Result<int, ApiDenyRequestError>.Success(0);
        public BoardRentAndProperty.Api.Services.Result<int, ApiCancelRequestError> CancelOutcome { get; set; } =
            BoardRentAndProperty.Api.Services.Result<int, ApiCancelRequestError>.Success(0);
        public BoardRentAndProperty.Api.Services.Result<int, ApiOfferError> OfferOutcome { get; set; } =
            BoardRentAndProperty.Api.Services.Result<int, ApiOfferError>.Success(60);

        public int OnGameDeactivatedCallCount { get; private set; }
        public int LastDeactivatedGameId { get; private set; }

        public ImmutableList<RequestDTO> GetRequestsForRenter(Guid renterAccountId) => this.RenterRequests;

        public ImmutableList<RequestDTO> GetRequestsForOwner(Guid ownerAccountId) => this.OwnerRequests;

        public ImmutableList<RequestDTO> GetOpenRequestsForOwner(Guid ownerAccountId) => this.OpenOwnerRequests;

        public BoardRentAndProperty.Api.Services.Result<int, ApiCreateRequestError> CreateRequest(
            int gameId,
            Guid renterAccountId,
            Guid ownerAccountId,
            DateTime startDate,
            DateTime endDate) => this.CreateOutcome;

        public BoardRentAndProperty.Api.Services.Result<int, ApiApproveRequestError> ApproveRequest(int requestId, Guid ownerAccountId) =>
            this.ApproveOutcome;

        public BoardRentAndProperty.Api.Services.Result<int, ApiDenyRequestError> DenyRequest(int requestId, Guid ownerAccountId, string declineReason) =>
            this.DenyOutcome;

        public BoardRentAndProperty.Api.Services.Result<int, ApiCancelRequestError> CancelRequest(int requestId, Guid cancellingAccountId) =>
            this.CancelOutcome;

        public void OnGameDeactivated(int gameId)
        {
            this.OnGameDeactivatedCallCount++;
            this.LastDeactivatedGameId = gameId;
        }

        public bool CheckAvailability(int gameId, DateTime startDate, DateTime endDate) => this.AvailabilityResult;

        public ImmutableList<ApiBookedDateRange> GetBookedDates(int gameId, int calendarMonth, int calendarYear) => this.BookedDates;

        public BoardRentAndProperty.Api.Services.Result<int, ApiOfferError> OfferGame(int requestId, Guid offeringOwnerAccountId) =>
            this.OfferOutcome;
    }
}
