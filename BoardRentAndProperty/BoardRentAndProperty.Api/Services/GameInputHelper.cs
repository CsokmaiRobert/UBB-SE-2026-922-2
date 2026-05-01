using System;
using System.Collections.Generic;
using BoardRentAndProperty.Api.Constants;

namespace BoardRentAndProperty.Api.Services
{
    internal static class GameInputHelper
    {
        private const int EmptyImageLength = 0;

        public static List<string> BuildValidationErrors(
            string gameName,
            decimal gamePrice,
            int minimumPlayerCount,
            int maximumPlayerCount,
            string gameDescription,
            int minimumNameLength,
            int maximumNameLength,
            decimal minimumAllowedPrice,
            int absoluteMinimumPlayerCount,
            int minimumDescriptionLength,
            int maximumDescriptionLength)
        {
            var gameValidationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(gameName) || gameName.Length < minimumNameLength || gameName.Length > maximumNameLength)
            {
                gameValidationErrors.Add(ValidationMessages.NameLengthRange(minimumNameLength, maximumNameLength));
            }

            if (gamePrice < minimumAllowedPrice)
            {
                gameValidationErrors.Add(ValidationMessages.PriceMinimum(minimumAllowedPrice));
            }

            if (minimumPlayerCount < absoluteMinimumPlayerCount)
            {
                gameValidationErrors.Add(ValidationMessages.MinimumPlayerCount(absoluteMinimumPlayerCount));
            }

            if (maximumPlayerCount < minimumPlayerCount)
            {
                gameValidationErrors.Add(ValidationMessages.MaximumPlayerCountComparedToMinimum);
            }

            if (string.IsNullOrWhiteSpace(gameDescription) || gameDescription.Length < minimumDescriptionLength || gameDescription.Length > maximumDescriptionLength)
            {
                gameValidationErrors.Add(ValidationMessages.DescriptionLengthRange(minimumDescriptionLength, maximumDescriptionLength));
            }

            return gameValidationErrors;
        }

        public static byte[] EnsureImageOrDefault(byte[] gameImage, string applicationBaseDirectory)
        {
            if (gameImage != null && gameImage.Length > EmptyImageLength)
            {
                return gameImage;
            }

            try
            {
                string defaultGameImagePath = System.IO.Path.Combine(applicationBaseDirectory, "Assets", "default-game-placeholder.jpg");
                return System.IO.File.ReadAllBytes(defaultGameImagePath);
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }
    }
}
