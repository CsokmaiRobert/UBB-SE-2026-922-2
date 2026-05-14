using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.Tests.Fakes
{
    internal sealed class FakeClientAuthService : IAuthService
    {
        public ServiceResult<bool> RegisterResult { get; set; } = ServiceResult<bool>.Ok(true);
        public ServiceResult<AccountProfileDataTransferObject> LoginResult { get; set; } =
            ServiceResult<AccountProfileDataTransferObject>.Ok(new AccountProfileDataTransferObject());
        public ServiceResult<bool> LogoutResult { get; set; } = ServiceResult<bool>.Ok(true);
        public ServiceResult<string> ForgotPasswordResult { get; set; } = ServiceResult<string>.Ok(string.Empty);
        public int RegisterCallCount { get; private set; }
        public int LoginCallCount { get; private set; }
        public RegisterDataTransferObject? LastRegisterRequest { get; private set; }
        public LoginDataTransferObject? LastLoginRequest { get; private set; }

        public Task<ServiceResult<bool>> RegisterAsync(RegisterDataTransferObject dto)
        {
            this.RegisterCallCount++;
            this.LastRegisterRequest = dto;
            return Task.FromResult(this.RegisterResult);
        }

        public Task<ServiceResult<AccountProfileDataTransferObject>> LoginAsync(LoginDataTransferObject dto)
        {
            this.LoginCallCount++;
            this.LastLoginRequest = dto;
            return Task.FromResult(this.LoginResult);
        }

        public Task<ServiceResult<bool>> LogoutAsync() => Task.FromResult(this.LogoutResult);

        public Task<ServiceResult<string>> ForgotPasswordAsync() => Task.FromResult(this.ForgotPasswordResult);
    }

    internal sealed class FakeClientAdminService : IAdminService
    {
        public ServiceResult<List<AccountProfileDataTransferObject>> AccountsResult { get; set; } =
            ServiceResult<List<AccountProfileDataTransferObject>>.Ok(new List<AccountProfileDataTransferObject>());
        public ServiceResult<bool> SuspendResult { get; set; } = ServiceResult<bool>.Ok(true);
        public ServiceResult<bool> UnsuspendResult { get; set; } = ServiceResult<bool>.Ok(true);
        public ServiceResult<bool> ResetPasswordResult { get; set; } = ServiceResult<bool>.Ok(true);
        public ServiceResult<bool> UnlockResult { get; set; } = ServiceResult<bool>.Ok(true);
        public int GetAllAccountsCallCount { get; private set; }
        public int SuspendCallCount { get; private set; }
        public int UnsuspendCallCount { get; private set; }
        public int ResetPasswordCallCount { get; private set; }
        public int UnlockCallCount { get; private set; }
        public Guid LastAccountId { get; private set; }
        public int LastPage { get; private set; }
        public int LastPageSize { get; private set; }
        public string LastNewPassword { get; private set; } = string.Empty;

        public Task<ServiceResult<List<AccountProfileDataTransferObject>>> GetAllAccountsAsync(int page, int pageSize)
        {
            this.GetAllAccountsCallCount++;
            this.LastPage = page;
            this.LastPageSize = pageSize;
            return Task.FromResult(this.AccountsResult);
        }

        public Task<ServiceResult<bool>> SuspendAccountAsync(Guid accountId)
        {
            this.SuspendCallCount++;
            this.LastAccountId = accountId;
            return Task.FromResult(this.SuspendResult);
        }

        public Task<ServiceResult<bool>> UnsuspendAccountAsync(Guid accountId)
        {
            this.UnsuspendCallCount++;
            this.LastAccountId = accountId;
            return Task.FromResult(this.UnsuspendResult);
        }

        public Task<ServiceResult<bool>> ResetPasswordAsync(Guid accountId, string newPassword)
        {
            this.ResetPasswordCallCount++;
            this.LastAccountId = accountId;
            this.LastNewPassword = newPassword;
            return Task.FromResult(this.ResetPasswordResult);
        }

        public Task<ServiceResult<bool>> UnlockAccountAsync(Guid accountId)
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
        public ServiceResult<bool> UpdateProfileResult { get; set; } = ServiceResult<bool>.Ok(true);
        public ServiceResult<bool> ChangePasswordResult { get; set; } = ServiceResult<bool>.Ok(true);
        public string UploadedAvatarUrl { get; set; } = string.Empty;
        public int GetProfileCallCount { get; private set; }
        public int UpdateProfileCallCount { get; private set; }
        public int ChangePasswordCallCount { get; private set; }
        public int UploadAvatarCallCount { get; private set; }
        public int RemoveAvatarCallCount { get; private set; }
        public AccountProfileDataTransferObject? LastProfileUpdate { get; private set; }

        public Task<ServiceResult<AccountProfileDataTransferObject>> GetProfileAsync(Guid accountId)
        {
            this.GetProfileCallCount++;
            return Task.FromResult(this.ProfileResult);
        }

        public Task<ServiceResult<bool>> UpdateProfileAsync(Guid accountId, AccountProfileDataTransferObject profileUpdateData)
        {
            this.UpdateProfileCallCount++;
            this.LastProfileUpdate = profileUpdateData;
            return Task.FromResult(this.UpdateProfileResult);
        }

        public Task<ServiceResult<bool>> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword)
        {
            this.ChangePasswordCallCount++;
            return Task.FromResult(this.ChangePasswordResult);
        }

        public Task<string> UploadAvatarAsync(Guid accountId, string sourceFilePath)
        {
            this.UploadAvatarCallCount++;
            return Task.FromResult(this.UploadedAvatarUrl);
        }

        public Task RemoveAvatarAsync(Guid accountId)
        {
            this.RemoveAvatarCallCount++;
            return Task.CompletedTask;
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

        public void AddGame(GameDTO gameDto)
        {
            this.AddGameCallCount++;
            this.LastAddedGame = gameDto;
            if (this.AddGameException != null)
            {
                throw this.AddGameException;
            }
        }

        public void UpdateGameByIdentifier(int gameId, GameDTO updatedGameDTO)
        {
            this.UpdateGameCallCount++;
            this.LastUpdatedGameId = gameId;
            this.LastUpdatedGame = updatedGameDTO;
            if (this.UpdateGameException != null)
            {
                throw this.UpdateGameException;
            }
        }

        public GameDTO DeleteGameByIdentifier(int gameId)
        {
            this.DeleteGameCallCount++;
            this.LastDeletedGameId = gameId;
            if (this.DeleteGameException != null)
            {
                throw this.DeleteGameException;
            }

            return this.DeletedGameResult;
        }

        public GameDTO GetGameByIdentifier(int gameId) => this.GameToReturn;

        public ImmutableList<GameDTO> GetGamesForOwner(Guid ownerAccountId) => this.GamesForOwner;

        public ImmutableList<GameDTO> GetAllGames() => this.AllGames;

        public List<string> ValidateGame(GameDTO gameDto) =>
            this.ValidateGameHandler?.Invoke(gameDto) ?? new List<string>();

        public ImmutableList<GameDTO> GetAvailableGamesForRenter(Guid renterAccountId) =>
            this.AvailableGamesForRenter;

        public ImmutableList<GameDTO> GetActiveGamesForOwner(Guid ownerAccountId) =>
            this.ActiveGamesForOwner;
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

        public ImmutableList<RentalDTO> GetRentalsForRenter(Guid renterAccountId) => this.RentalsForRenter;

        public ImmutableList<RentalDTO> GetRentalsForOwner(Guid ownerAccountId) => this.RentalsForOwner;

        public bool IsSlotAvailable(int gameId, DateTime requestedStartDate, DateTime requestedEndDate) =>
            this.SlotAvailable;

        public void CreateConfirmedRental(int gameId, Guid renterAccountId, Guid ownerAccountId, DateTime startDate, DateTime endDate)
        {
            this.CreateRentalCallCount++;
            this.LastGameId = gameId;
            this.LastRenterAccountId = renterAccountId;
            this.LastOwnerAccountId = ownerAccountId;
            if (this.CreateRentalException != null)
            {
                throw this.CreateRentalException;
            }
        }
    }

    internal sealed class FakeClientUserService : IUserService
    {
        public ImmutableList<UserDTO> UsersExceptCurrent { get; set; } = ImmutableList<UserDTO>.Empty;

        public ImmutableList<UserDTO> GetUsersExcept(Guid excludeAccountId) => this.UsersExceptCurrent;
    }
}
