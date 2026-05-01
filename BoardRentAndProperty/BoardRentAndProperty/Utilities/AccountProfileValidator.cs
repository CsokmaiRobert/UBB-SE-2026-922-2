namespace BoardRentAndProperty.Utilities
{
    using System.Collections.Generic;
    using BoardRentAndProperty.DataTransferObjects;

    public static class AccountProfileValidator
    {
        private const int MinimumDisplayNameLength = 2;
        private const int MaximumDisplayNameLength = 50;
        private const int MaximumStreetNumberLength = 10;

        public static List<string> Validate(AccountProfileDataTransferObject profileData)
        {
            List<string> validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(profileData.DisplayName) ||
                profileData.DisplayName.Length < MinimumDisplayNameLength ||
                profileData.DisplayName.Length > MaximumDisplayNameLength)
            {
                validationErrors.Add("DisplayName|Display name must be between 2 and 50 characters long.");
            }

            if (!string.IsNullOrWhiteSpace(profileData.PhoneNumber))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(profileData.PhoneNumber, @"^\+?\d{7,15}$"))
                {
                    validationErrors.Add("PhoneNumber|Phone number format is invalid.");
                }
            }

            if (!string.IsNullOrWhiteSpace(profileData.StreetNumber) && profileData.StreetNumber.Length > MaximumStreetNumberLength)
            {
                validationErrors.Add("StreetNumber|Street number must be a valid value.");
            }

            return validationErrors;
        }
    }
}