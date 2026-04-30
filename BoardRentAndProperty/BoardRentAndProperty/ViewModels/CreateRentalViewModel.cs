using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Models;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.ViewModels
{
    public class CreateRentalViewModel : INotifyPropertyChanged
    {
        private const string ValidationFailedMessage = "Validation failed.";

        private readonly IGameService gameListingService;
        private readonly IRentalService rentalCreationService;
        private readonly IAccountService accountService;
        private readonly ICurrentUserContext currentUserContext;

        public int CurrentUserId => currentUserContext.CurrentUserId;

        public ObservableCollection<GameDTO> OwnedActiveGames { get; set; } = new();
        public ObservableCollection<Account> AvailableRenters { get; set; } = new();

        private GameDTO selectedGameToRent;
        public GameDTO SelectedGameToRent
        {
            get => selectedGameToRent;
            set
            {
                selectedGameToRent = value;
                OnPropertyChanged();
            }
        }

        private Account selectedRenter;
        public Account SelectedRenter
        {
            get => selectedRenter;
            set
            {
                selectedRenter = value;
                OnPropertyChanged();
            }
        }

        private DateTimeOffset? rentalStartDate;
        public DateTimeOffset? StartDate
        {
            get => rentalStartDate;
            set
            {
                rentalStartDate = value;
                OnPropertyChanged();
            }
        }

        private DateTimeOffset? rentalEndDate;
        public DateTimeOffset? EndDate
        {
            get => rentalEndDate;
            set
            {
                rentalEndDate = value;
                OnPropertyChanged();
            }
        }

        public CreateRentalViewModel(IGameService gameListingService, IRentalService rentalCreationService,
                                     IAccountService accountService, ICurrentUserContext currentUserContext)
        {
            this.gameListingService = gameListingService;
            this.rentalCreationService = rentalCreationService;
            this.accountService = accountService;
            this.currentUserContext = currentUserContext;
            _ = LoadRentalFormDataAsync();
        }

        public async System.Threading.Tasks.Task LoadRentalFormDataAsync()
        {
            OwnedActiveGames.Clear();
            foreach (var activeGame in gameListingService.GetActiveGamesForOwner(CurrentUserId))
            {
                OwnedActiveGames.Add(activeGame);
            }

            AvailableRenters.Clear();
            ServiceResult<System.Collections.Generic.List<Account>> renterListResult = await accountService.GetAccountsExceptPamUserIdAsync(CurrentUserId);
            if (!renterListResult.Success || renterListResult.Data == null)
            {
                return;
            }

            foreach (var potentialRenter in renterListResult.Data)
            {
                AvailableRenters.Add(potentialRenter);
            }
        }

        public bool ValidateRentalInputs()
        {
            if (SelectedGameToRent == null)
            {
                return false;
            }

            if (SelectedRenter == null)
            {
                return false;
            }

            return StartDate != null && EndDate != null;
        }

        public ViewOperationResult CreateRental()
        {
            if (!ValidateRentalInputs())
            {
                return ViewOperationResult.Failure(
                    Constants.DialogTitles.ValidationError,
                    Constants.DialogMessages.CreateRentalValidationError);
            }

            try
            {
                rentalCreationService.CreateConfirmedRental(
                    SelectedGameToRent.Id,
                    SelectedRenter.PamUserId ?? 0,
                    CurrentUserId,
                    StartDate.Value.DateTime,
                    EndDate.Value.DateTime);
                return ViewOperationResult.Success();
            }
            catch (ArgumentException)
            {
                return ViewOperationResult.Failure(
                    Constants.DialogTitles.ValidationError,
                    Constants.DialogMessages.CreateRentalValidationError);
            }
            catch (Exception rentalCreationException)
            {
                return ViewOperationResult.Failure(Constants.DialogTitles.RentalFailed, rentalCreationException.Message);
            }
        }

        public string? SaveRental()
        {
            var rentalCreationResult = CreateRental();
            if (rentalCreationResult.IsSuccess)
            {
                return null;
            }

            if (rentalCreationResult.DialogTitle == Constants.DialogTitles.ValidationError)
            {
                return ValidationFailedMessage;
            }

            return rentalCreationResult.DialogMessage;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
