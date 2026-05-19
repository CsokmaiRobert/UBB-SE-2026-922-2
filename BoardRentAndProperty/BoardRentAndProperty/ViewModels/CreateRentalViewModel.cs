using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BoardRentAndProperty.ApiClient;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Utilities;

namespace BoardRentAndProperty.ViewModels
{
    public class CreateRentalViewModel : INotifyPropertyChanged
    {
        private const string ValidationFailedMessage = "Validation failed.";

        private readonly IGameService gameListingService;
        private readonly IRentalService rentalCreationService;
        private readonly IUserService userService;
        private readonly ICurrentUserContext currentUserContext;

        public Guid CurrentUserId => currentUserContext.CurrentUserId;

        public ObservableCollection<GameDTO> OwnedActiveGames { get; set; } = new();
        public ObservableCollection<UserDTO> AvailableRenters { get; set; } = new();

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

        private UserDTO selectedRenter;
        public UserDTO SelectedRenter
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
                                     IUserService userService, ICurrentUserContext currentUserContext)
        {
            this.gameListingService = gameListingService;
            this.rentalCreationService = rentalCreationService;
            this.userService = userService;
            this.currentUserContext = currentUserContext;
            _ = LoadRentalFormDataAsync();
        }

        public async Task LoadRentalFormDataAsync()
        {
            OwnedActiveGames.Clear();
            var activeGamesResult = await this.gameListingService.GetActiveGamesForOwnerAsync(CurrentUserId);
            if (activeGamesResult.Success && activeGamesResult.Data != null)
            {
                foreach (var activeGame in activeGamesResult.Data)
                {
                    OwnedActiveGames.Add(activeGame);
                }
            }

            AvailableRenters.Clear();
            var rentersResult = await this.userService.GetUsersExceptAsync(CurrentUserId);
            if (rentersResult.Success && rentersResult.Data != null)
            {
                foreach (var potentialRenter in rentersResult.Data)
                {
                    AvailableRenters.Add(potentialRenter);
                }
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

        public async Task<ViewOperationResult> CreateRentalAsync()
        {
            if (!ValidateRentalInputs())
            {
                return ViewOperationResult.Failure(
                    Constants.DialogTitles.ValidationError,
                    Constants.DialogMessages.CreateRentalValidationError);
            }

            var rentalDataTransferObject = new CreateRentalDataTransferObject
            {
                GameId = SelectedGameToRent.Id,
                RenterAccountId = SelectedRenter.Id,
                OwnerAccountId = CurrentUserId,
                StartDate = StartDate.Value.DateTime,
                EndDate = EndDate.Value.DateTime,
            };

            var rentalCreationResult = await this.rentalCreationService.CreateConfirmedRentalAsync(rentalDataTransferObject);
            if (rentalCreationResult.Success)
            {
                return ViewOperationResult.Success();
            }

            if (rentalCreationResult.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return ViewOperationResult.Failure(
                    Constants.DialogTitles.ValidationError,
                    Constants.DialogMessages.CreateRentalValidationError);
            }

            return ViewOperationResult.Failure(
                Constants.DialogTitles.RentalFailed,
                string.IsNullOrWhiteSpace(rentalCreationResult.Error)
                    ? Constants.DialogMessages.UnexpectedErrorOccurred
                    : rentalCreationResult.Error);
        }

        public async Task<string?> SaveRentalAsync()
        {
            var rentalCreationResult = await this.CreateRentalAsync();
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
