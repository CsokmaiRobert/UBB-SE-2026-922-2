using System;
using System.Collections.Generic;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.ViewModels;
using Moq;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class EditGameViewModelTests
    {
        private const int SampleGameIdentifier = 42;

        private readonly Guid sampleOwnerIdentifier = Guid.NewGuid();
        private Mock<IGameService> gameServiceMock = null!;
        private EditGameViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            this.gameServiceMock = new Mock<IGameService>();
            this.gameServiceMock
                .Setup(service => service.ValidateGame(It.IsAny<GameDTO>()))
                .Returns(new List<string>());
            this.viewModel = new EditGameViewModel(this.gameServiceMock.Object);
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

            this.gameServiceMock
                .Setup(service => service.GetGameByIdentifier(SampleGameIdentifier))
                .Returns(existingGame);

            this.viewModel.LoadGame(SampleGameIdentifier);

            Assert.That(this.viewModel.EditedGameId, Is.EqualTo(SampleGameIdentifier));
            Assert.That(this.viewModel.GameName, Is.EqualTo("Existing Game"));
        }

        [Test]
        public void UpdateGame_ValidInputs_CallsUpdateWithCorrectIdentifier()
        {
            this.gameServiceMock
                .Setup(service => service.GetGameByIdentifier(SampleGameIdentifier))
                .Returns(new GameDTO
                {
                    Id = SampleGameIdentifier,
                    Owner = new UserDTO { Id = this.sampleOwnerIdentifier },
                    Name = "Valid Name",
                    Price = 10m,
                    MinimumPlayerNumber = 2,
                    MaximumPlayerNumber = 4,
                    Description = "This description is long enough to pass the validation rules.",
                    IsActive = true,
                });

            this.viewModel.LoadGame(SampleGameIdentifier);
            this.viewModel.UpdateGame();

            this.gameServiceMock.Verify(service => service.UpdateGameByIdentifier(
                SampleGameIdentifier,
                It.IsAny<GameDTO>()), Times.Once);
        }
    }
}
