namespace BoardRentAndProperty.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using BoardRentAndProperty.Constants;
    using BoardRentAndProperty.DataTransferObjects;
    using BoardRentAndProperty.Services;
    using BoardRentAndProperty.Utilities;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;

    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService authService;

        [ObservableProperty]
        private string displayName = string.Empty;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private string phoneNumber = string.Empty;

        [ObservableProperty]
        private string country = string.Empty;

        [ObservableProperty]
        private string city = string.Empty;

        [ObservableProperty]
        private string streetName = string.Empty;

        [ObservableProperty]
        private string streetNumber = string.Empty;

        [ObservableProperty]
        private string displayNameError = string.Empty;

        [ObservableProperty]
        private string usernameError = string.Empty;

        [ObservableProperty]
        private string emailError = string.Empty;

        [ObservableProperty]
        private string passwordError = string.Empty;

        [ObservableProperty]
        private string confirmPasswordError = string.Empty;

        [ObservableProperty]
        private string phoneNumberError = string.Empty;

        public RegisterViewModel(IAuthService authService)
        {
            this.authService = authService;
        }

        public Action OnRegistrationSuccess { get; set; }

        public Action OnNavigateBackRequest { get; set; }

        public IReadOnlyList<string> AvailableCountries => DomainConstants.CountryList;

        private void ClearErrors()
        {
            this.DisplayNameError = string.Empty;
            this.UsernameError = string.Empty;
            this.EmailError = string.Empty;
            this.PasswordError = string.Empty;
            this.ConfirmPasswordError = string.Empty;
            this.PhoneNumberError = string.Empty;
            this.ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            this.ClearErrors();

            this.IsLoading = true;

            RegisterDataTransferObject registrationRequest = new RegisterDataTransferObject
            {
                DisplayName = this.DisplayName,
                Username = this.Username,
                Email = this.Email,
                Password = this.Password,
                ConfirmPassword = this.ConfirmPassword,
                PhoneNumber = this.PhoneNumber,
                Country = this.Country,
                City = this.City,
                StreetName = this.StreetName,
                StreetNumber = this.StreetNumber,
            };

            ServiceResult<bool> registrationResult = await this.authService.RegisterAsync(registrationRequest);

            if (registrationResult.Success)
            {
                this.OnRegistrationSuccess?.Invoke();
            }
            else
            {
                const int MaximumSplitSubstrings = 2;
                string[] parsedFieldErrors = registrationResult.Error.Split(';', StringSplitOptions.RemoveEmptyEntries);

                foreach (string fieldError in parsedFieldErrors)
                {
                    string[] errorComponents = fieldError.Split('|', MaximumSplitSubstrings);

                    if (errorComponents.Length == MaximumSplitSubstrings)
                    {
                        string fieldName = errorComponents[0];
                        string errorMessageText = errorComponents[1];

                        switch (fieldName)
                        {
                            case "DisplayName": this.DisplayNameError = errorMessageText; break;
                            case "Username": this.UsernameError = errorMessageText; break;
                            case "Email": this.EmailError = errorMessageText; break;
                            case "Password": this.PasswordError = errorMessageText; break;
                            case "ConfirmPassword": this.ConfirmPasswordError = errorMessageText; break;
                            case "PhoneNumber": this.PhoneNumberError = errorMessageText; break;
                            default: this.ErrorMessage = errorMessageText; break;
                        }
                    }
                    else
                    {
                        this.ErrorMessage = fieldError;
                    }
                }
            }

            this.IsLoading = false;
        }

        [RelayCommand]
        private void GoToLogin()
        {
            this.OnNavigateBackRequest?.Invoke();
        }
    }
}
