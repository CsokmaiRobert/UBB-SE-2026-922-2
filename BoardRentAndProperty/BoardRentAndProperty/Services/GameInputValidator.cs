using System;
using System.Collections.Generic;
using BoardRentAndProperty.Constants;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardRentAndProperty.Services
{
    public static class GameInputValidator
    {
        public static List<string> Validate(GameDTO game)
        {
            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(game.Name)
                || game.Name.Length < DomainConstants.GameMinimumNameLength
                || game.Name.Length > DomainConstants.GameMaximumNameLength)
            {
                validationErrors.Add($"Name must be between {DomainConstants.GameMinimumNameLength} and {DomainConstants.GameMaximumNameLength} characters.");
            }

            if (game.Price < DomainConstants.GameMinimumAllowedPrice)
            {
                validationErrors.Add($"Price must be greater than or equal to {DomainConstants.GameMinimumAllowedPrice:0}.");
            }

            if (game.MinimumPlayerNumber < DomainConstants.GameMinimumPlayerCount)
            {
                validationErrors.Add($"Minimum player count must be at least {DomainConstants.GameMinimumPlayerCount}.");
            }

            if (game.MaximumPlayerNumber < game.MinimumPlayerNumber)
            {
                validationErrors.Add("Maximum player count must be greater than or equal to minimum player count.");
            }

            if (string.IsNullOrWhiteSpace(game.Description)
                || game.Description.Length < DomainConstants.GameMinimumDescriptionLength
                || game.Description.Length > DomainConstants.GameMaximumDescriptionLength)
            {
                validationErrors.Add($"Description must be between {DomainConstants.GameMinimumDescriptionLength} and {DomainConstants.GameMaximumDescriptionLength} characters.");
            }

            return validationErrors;
        }
    }
}
