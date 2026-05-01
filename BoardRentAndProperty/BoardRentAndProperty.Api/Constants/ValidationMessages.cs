namespace BoardRentAndProperty.Api.Constants
{
    public static class ValidationMessages
    {
        public const string MaximumPlayerCountComparedToMinimum =
            "Maximum player count must be greater than or equal to minimum player count.";

        public static string NameLengthRange(int minimumLength, int maximumLength) =>
            $"Name must be between {minimumLength} and {maximumLength} characters.";

        public static string PriceMinimum(decimal minimumPrice) =>
            $"Price must be greater than or equal to {minimumPrice:0}.";

        public static string MinimumPlayerCount(int minimumPlayers) =>
            $"Minimum player count must be at least {minimumPlayers}.";

        public static string DescriptionLengthRange(int minimumLength, int maximumLength) =>
            $"Description must be between {minimumLength} and {maximumLength} characters.";
    }
}
