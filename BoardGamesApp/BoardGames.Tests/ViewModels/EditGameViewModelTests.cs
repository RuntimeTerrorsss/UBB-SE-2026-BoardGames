using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
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
            gameService = new FakeClientGameService
            {
                ValidateGameHandler = game => new List<string>(),
            };
            viewModel = new EditGameViewModel(gameService);
        }

        [Test]
        public async Task LoadGame_PopulatesPropertiesFromService()
        {
            var existingGame = new GameDTO
            {
                Id = SampleGameIdentifier,
                Owner = new UserDTO { Id = sampleOwnerIdentifier },
                Name = "Existing Game",
                Price = 15m,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 5,
                Description = "A very long description that passes validation in the current project.",
                IsActive = true,
            };

            gameService.GameToReturn = existingGame;

            await viewModel.LoadGameAsync(SampleGameIdentifier);

            Assert.That(viewModel.EditedGameId, Is.EqualTo(SampleGameIdentifier));
            Assert.That(viewModel.GameName, Is.EqualTo("Existing Game"));
        }

        [Test]
        public async Task UpdateGame_ValidInputs_CallsUpdateWithCorrectIdentifier()
        {
            gameService.GameToReturn = new GameDTO
                {
                    Id = SampleGameIdentifier,
                    Owner = new UserDTO { Id = sampleOwnerIdentifier },
                    Name = "Valid Name",
                    Price = 10m,
                    MinimumPlayerNumber = 2,
                    MaximumPlayerNumber = 4,
                    Description = "This description is long enough to pass the validation rules.",
                    IsActive = true,
                };

            await viewModel.LoadGameAsync(SampleGameIdentifier);
            await viewModel.UpdateGameAsync();

            Assert.That(gameService.UpdateGameCallCount, Is.EqualTo(1));
            Assert.That(gameService.LastUpdatedGameId, Is.EqualTo(SampleGameIdentifier));
        }
    }
}
