using System;
using System.Collections.Generic;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Tests.Fakes;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class EditGameViewModelTests
    {
        private const int SampleGameIdentifier = 42;

        private readonly Guid sampleOwnerIdentifier = Guid.NewGuid();
        private FakeClientGameService gameService = null!;
        private EditGameViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            this.gameService = new FakeClientGameService
            {
                ValidateGameHandler = _ => new List<string>(),
            };
            this.viewModel = new EditGameViewModel(this.gameService);
        }

        [Test]
        public void LoadGame_PopulatesPropertiesFromService()
        {
            var existingGame = new GameDTO
            {
                Id = SampleGameIdentifier,
                Owner = new UserDTO { Id = this.sampleOwnerIdentifier },
                Name = "Existing Game",
                Price = 15m,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 5,
                Description = "A very long description that passes validation in the current project.",
                IsActive = true,
            };

            this.gameService.GameToReturn = existingGame;

            this.viewModel.LoadGame(SampleGameIdentifier);

            Assert.That(this.viewModel.EditedGameId, Is.EqualTo(SampleGameIdentifier));
            Assert.That(this.viewModel.GameName, Is.EqualTo("Existing Game"));
        }

        [Test]
        public void UpdateGame_ValidInputs_CallsUpdateWithCorrectIdentifier()
        {
            this.gameService.GameToReturn = new GameDTO
                {
                    Id = SampleGameIdentifier,
                    Owner = new UserDTO { Id = this.sampleOwnerIdentifier },
                    Name = "Valid Name",
                    Price = 10m,
                    MinimumPlayerNumber = 2,
                    MaximumPlayerNumber = 4,
                    Description = "This description is long enough to pass the validation rules.",
                    IsActive = true,
                };

            this.viewModel.LoadGame(SampleGameIdentifier);
            this.viewModel.UpdateGame();

            Assert.That(this.gameService.UpdateGameCallCount, Is.EqualTo(1));
            Assert.That(this.gameService.LastUpdatedGameId, Is.EqualTo(SampleGameIdentifier));
        }
    }
}
