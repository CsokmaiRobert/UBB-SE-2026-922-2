using System.Collections.Generic;

namespace BoardRentAndProperty.Constants
{
    public static class DomainConstants
    {
        public const int RentalBufferHours = 48;

        public const int GameMinimumNameLength = 5;
        public const int GameMaximumNameLength = 30;
        public const decimal GameMinimumAllowedPrice = 1m;
        public const int GameMinimumPlayerCount = 1;
        public const int GameMinimumDescriptionLength = 10;
        public const int GameMaximumDescriptionLength = 500;
        public const int GameDefaultMinimumPlayers = 1;
        public const int GameDefaultMaximumPlayers = 4;

        public const string ApplicationName = "BoardRentAndProperty";
        public const string AvatarFolderName = "Avatars";

        public static readonly IReadOnlyList<string> CountryList = new List<string>
        {
            "Argentina", "Australia", "Austria", "Belgium", "Brazil", "Canada",
            "China", "Colombia", "Denmark", "Egypt", "Finland", "France",
            "Germany", "Greece", "Hungary", "India", "Ireland", "Italy",
            "Japan", "Mexico", "Netherlands", "New Zealand", "Norway", "Poland",
            "Portugal", "Romania", "South Africa", "Spain", "Sweden", "Switzerland",
            "Turkey", "United Kingdom", "United States"
        };
    }
}
