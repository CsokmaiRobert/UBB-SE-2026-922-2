using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;

namespace BoardRentAndProperty.Tests.Fakes
{
    internal sealed class FakeClientAuthService : IAuthService
    {
        public ServiceResult RegisterResult { get; set; } = ServiceResult.Ok();
        public ServiceResult<AccountProfileDataTransferObject> LoginResult { get; set; } =
            ServiceResult<AccountProfileDataTransferObject>.Ok(new AccountProfileDataTransferObject());
        public ServiceResult LogoutResult { get; set; } = ServiceResult.Ok();
        public ServiceResult<string> ForgotPasswordResult { get; set; } = ServiceResult<string>.Ok(string.Empty);
        public int RegisterCallCount { get; private set; }
        public int LoginCallCount { get; private set; }
        public RegisterDataTransferObject? LastRegisterRequest { get; private set; }
        public LoginDataTransferObject? LastLoginRequest { get; private set; }

        public Task<ServiceResult> RegisterAsync(
            RegisterDataTransferObject request,
            CancellationToken cancellationToken = default)
        {
            this.RegisterCallCount++;
            this.LastRegisterRequest = request;
            return Task.FromResult(this.RegisterResult);
        }

        public Task<ServiceResult<AccountProfileDataTransferObject>> LoginAsync(
            LoginDataTransferObject request,
            CancellationToken cancellationToken = default)
        {
            this.LoginCallCount++;
            this.LastLoginRequest = request;
            return Task.FromResult(this.LoginResult);
        }

        public Task<ServiceResult> LogoutAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(this.LogoutResult);

        public Task<ServiceResult<string>> ForgotPasswordAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(this.ForgotPasswordResult);
    }

    internal sealed class FakeClientAdminService : IAdminService
    {
        public ServiceResult<List<AccountProfileDataTransferObject>> AccountsResult { get; set; } =
            ServiceResult<List<AccountProfileDataTransferObject>>.Ok(new List<AccountProfileDataTransferObject>());
        public ServiceResult SuspendResult { get; set; } = ServiceResult.Ok();
        public ServiceResult UnsuspendResult { get; set; } = ServiceResult.Ok();
        public ServiceResult ResetPasswordResult { get; set; } = ServiceResult.Ok();
        public ServiceResult UnlockResult { get; set; } = ServiceResult.Ok();
        public int GetAllAccountsCallCount { get; private set; }
        public int SuspendCallCount { get; private set; }
        public int UnsuspendCallCount { get; private set; }
        public int ResetPasswordCallCount { get; private set; }
        public int UnlockCallCount { get; private set; }
        public Guid LastAccountId { get; private set; }
        public int LastPage { get; private set; }
        public int LastPageSize { get; private set; }
        public string LastNewPassword { get; private set; } = string.Empty;

        public Task<ServiceResult<IReadOnlyList<AccountProfileDataTransferObject>>> GetAllAccountsAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            this.GetAllAccountsCallCount++;
            this.LastPage = page;
            this.LastPageSize = pageSize;
            return Task.FromResult(this.AccountsResult.Success
                ? ServiceResult<IReadOnlyList<AccountProfileDataTransferObject>>.Ok(
                    this.AccountsResult.Data ?? new List<AccountProfileDataTransferObject>())
                : ServiceResult<IReadOnlyList<AccountProfileDataTransferObject>>.Fail(this.AccountsResult));
        }

        public Task<ServiceResult> SuspendAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            this.SuspendCallCount++;
            this.LastAccountId = accountId;
            return Task.FromResult(this.SuspendResult);
        }

        public Task<ServiceResult> UnsuspendAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            this.UnsuspendCallCount++;
            this.LastAccountId = accountId;
            return Task.FromResult(this.UnsuspendResult);
        }

        public Task<ServiceResult> ResetPasswordAsync(
            Guid accountId,
            string newPassword,
            CancellationToken cancellationToken = default)
        {
            this.ResetPasswordCallCount++;
            this.LastAccountId = accountId;
            this.LastNewPassword = newPassword;
            return Task.FromResult(this.ResetPasswordResult);
        }

        public Task<ServiceResult> UnlockAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            this.UnlockCallCount++;
            this.LastAccountId = accountId;
            return Task.FromResult(this.UnlockResult);
        }
    }

    internal sealed class FakeClientAccountService : IAccountService
    {
        public ServiceResult<AccountProfileDataTransferObject> ProfileResult { get; set; } =
            ServiceResult<AccountProfileDataTransferObject>.Ok(new AccountProfileDataTransferObject());
        public ServiceResult UpdateProfileResult { get; set; } = ServiceResult.Ok();
        public ServiceResult ChangePasswordResult { get; set; } = ServiceResult.Ok();
        public ServiceResult<string> UploadAvatarResult { get; set; } = ServiceResult<string>.Ok(string.Empty);
        public string UploadedAvatarUrl
        {
            get => this.UploadAvatarResult.Data ?? string.Empty;
            set => this.UploadAvatarResult = ServiceResult<string>.Ok(value);
        }

        public int GetProfileCallCount { get; private set; }
        public int UpdateProfileCallCount { get; private set; }
        public int ChangePasswordCallCount { get; private set; }
        public int UploadAvatarCallCount { get; private set; }
        public int RemoveAvatarCallCount { get; private set; }
        public AccountProfileDataTransferObject? LastProfileUpdate { get; private set; }

        public Task<ServiceResult<AccountProfileDataTransferObject>> GetProfileAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            this.GetProfileCallCount++;
            return Task.FromResult(this.ProfileResult);
        }

        public Task<ServiceResult> UpdateProfileAsync(
            Guid accountId,
            AccountProfileDataTransferObject profileUpdateData,
            CancellationToken cancellationToken = default)
        {
            this.UpdateProfileCallCount++;
            this.LastProfileUpdate = profileUpdateData;
            return Task.FromResult(this.UpdateProfileResult);
        }

        public Task<ServiceResult> ChangePasswordAsync(
            Guid accountId,
            string currentPassword,
            string newPassword,
            CancellationToken cancellationToken = default)
        {
            this.ChangePasswordCallCount++;
            return Task.FromResult(this.ChangePasswordResult);
        }

        public Task<ServiceResult<string>> UploadAvatarAsync(
            Guid accountId,
            string sourceFilePath,
            CancellationToken cancellationToken = default)
        {
            this.UploadAvatarCallCount++;
            return Task.FromResult(this.UploadAvatarResult);
        }

        public Task<ServiceResult> RemoveAvatarAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            this.RemoveAvatarCallCount++;
            return Task.FromResult(ServiceResult.Ok());
        }
    }

    internal sealed class FakeFilePickerService : IFilePickerService
    {
        public string SelectedPath { get; set; } = string.Empty;

        public Task<string> PickImageFileAsync() => Task.FromResult(this.SelectedPath);
    }

    internal sealed class FakeClientGameService : IGameService
    {
        public ImmutableList<GameDTO> GamesForOwner { get; set; } = ImmutableList<GameDTO>.Empty;
        public ImmutableList<GameDTO> AllGames { get; set; } = ImmutableList<GameDTO>.Empty;
        public ImmutableList<GameDTO> AvailableGamesForRenter { get; set; } = ImmutableList<GameDTO>.Empty;
        public ImmutableList<GameDTO> ActiveGamesForOwner { get; set; } = ImmutableList<GameDTO>.Empty;
        public GameDTO GameToReturn { get; set; } = new GameDTO();
        public GameDTO DeletedGameResult { get; set; } = new GameDTO();
        public Func<GameDTO, List<string>>? ValidateGameHandler { get; set; }
        public Exception? AddGameException { get; set; }
        public Exception? UpdateGameException { get; set; }
        public Exception? DeleteGameException { get; set; }
        public int AddGameCallCount { get; private set; }
        public int UpdateGameCallCount { get; private set; }
        public int DeleteGameCallCount { get; private set; }
        public int LastUpdatedGameId { get; private set; }
        public int LastDeletedGameId { get; private set; }
        public GameDTO? LastAddedGame { get; private set; }
        public GameDTO? LastUpdatedGame { get; private set; }

        public Task<ServiceResult> CreateGameAsync(GameDTO game, CancellationToken cancellationToken = default)
        {
            this.AddGameCallCount++;
            this.LastAddedGame = game;
            return Task.FromResult(this.AddGameException == null
                ? ServiceResult.Ok()
                : ServiceResult.Fail(this.AddGameException.Message));
        }

        public Task<ServiceResult> UpdateGameAsync(
            int gameId,
            GameDTO game,
            CancellationToken cancellationToken = default)
        {
            this.UpdateGameCallCount++;
            this.LastUpdatedGameId = gameId;
            this.LastUpdatedGame = game;
            return Task.FromResult(this.UpdateGameException == null
                ? ServiceResult.Ok()
                : ServiceResult.Fail(this.UpdateGameException.Message));
        }

        public Task<ServiceResult<GameDTO>> DeleteGameAsync(
            int gameId,
            CancellationToken cancellationToken = default)
        {
            this.DeleteGameCallCount++;
            this.LastDeletedGameId = gameId;
            return Task.FromResult(this.DeleteGameException == null
                ? ServiceResult<GameDTO>.Ok(this.DeletedGameResult)
                : ServiceResult<GameDTO>.Fail(this.DeleteGameException.Message));
        }

        public Task<ServiceResult<GameDTO>> GetGameByIdAsync(
            int gameId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ServiceResult<GameDTO>.Ok(this.GameToReturn));

        public Task<ServiceResult<IReadOnlyList<GameDTO>>> GetGamesForOwnerAsync(
            Guid ownerAccountId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ToReadOnlyListResult(this.GamesForOwner));

        public Task<ServiceResult<IReadOnlyList<GameDTO>>> GetAllGamesAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ToReadOnlyListResult(this.AllGames));

        public Task<ServiceResult<IReadOnlyList<GameDTO>>> GetAvailableGamesForRenterAsync(
            Guid renterAccountId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ToReadOnlyListResult(this.AvailableGamesForRenter));

        public Task<ServiceResult<IReadOnlyList<GameDTO>>> GetActiveGamesForOwnerAsync(
            Guid ownerAccountId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ToReadOnlyListResult(this.ActiveGamesForOwner));

        private static ServiceResult<IReadOnlyList<GameDTO>> ToReadOnlyListResult(
            IReadOnlyList<GameDTO> games) =>
            ServiceResult<IReadOnlyList<GameDTO>>.Ok(games);
    }

    internal sealed class FakeClientRentalService : IRentalService
    {
        public ImmutableList<RentalDTO> RentalsForRenter { get; set; } = ImmutableList<RentalDTO>.Empty;
        public ImmutableList<RentalDTO> RentalsForOwner { get; set; } = ImmutableList<RentalDTO>.Empty;
        public bool SlotAvailable { get; set; } = true;
        public Exception? CreateRentalException { get; set; }
        public int CreateRentalCallCount { get; private set; }
        public int LastGameId { get; private set; }
        public Guid LastRenterAccountId { get; private set; }
        public Guid LastOwnerAccountId { get; private set; }

        public Task<ServiceResult<IReadOnlyList<RentalDTO>>> GetRentalsForRenterAsync(
            Guid renterAccountId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ServiceResult<IReadOnlyList<RentalDTO>>.Ok(this.RentalsForRenter));

        public Task<ServiceResult<IReadOnlyList<RentalDTO>>> GetRentalsForOwnerAsync(
            Guid ownerAccountId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ServiceResult<IReadOnlyList<RentalDTO>>.Ok(this.RentalsForOwner));

        public Task<ServiceResult<bool>> IsSlotAvailableAsync(
            int gameId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ServiceResult<bool>.Ok(this.SlotAvailable));

        public Task<ServiceResult> CreateConfirmedRentalAsync(
            CreateRentalDataTransferObject rental,
            CancellationToken cancellationToken = default)
        {
            this.CreateRentalCallCount++;
            this.LastGameId = rental.GameId;
            this.LastRenterAccountId = rental.RenterAccountId;
            this.LastOwnerAccountId = rental.OwnerAccountId;
            return Task.FromResult(this.CreateRentalException == null
                ? ServiceResult.Ok()
                : ServiceResult.Fail(this.CreateRentalException.Message));
        }
    }

    internal sealed class FakeClientUserService : IUserService
    {
        public ImmutableList<UserDTO> UsersExceptCurrent { get; set; } = ImmutableList<UserDTO>.Empty;

        public Task<ServiceResult<IReadOnlyList<UserDTO>>> GetUsersExceptAsync(
            Guid excludeAccountId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ServiceResult<IReadOnlyList<UserDTO>>.Ok(this.UsersExceptCurrent));
    }
}
