using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using BoardRentAndProperty.Constants;
using BoardGames.Shared.DTO;
using BoardGames.Desktop.Services;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class CreateGameViewModelTests
    {
        private readonly Guid testUserId = Guid.NewGuid();
        private FakeClientGameService gameService = null!;
        private FakeCurrentUserContext currentUserContext = null!;
        private CreateGameViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            gameService = new FakeClientGameService
            {
                ValidateGameHandler = game => BoardGames.Api.Services.GameInputHelper.BuildValidationErrors(
                    game.Name,
                    game.Price,
                    game.MinimumPlayerNumber,
                    game.MaximumPlayerNumber,
                    game.Description,
                    DomainConstants.GameMinimumNameLength,
                    DomainConstants.GameMaximumNameLength,
                    DomainConstants.GameMinimumAllowedPrice,
                    DomainConstants.GameMinimumPlayerCount,
                    DomainConstants.GameMinimumDescriptionLength,
                    DomainConstants.GameMaximumDescriptionLength),
            };
            currentUserContext = new FakeCurrentUserContext { CurrentUserId = testUserId };

            viewModel = new CreateGameViewModel(gameService, currentUserContext);
        }

        [Test]
        public void Constructor_InitializesCurrentUserAndDefaultState()
        {
            Assert.Multiple(() =>
            {
                Assert.That(viewModel.CurrentUserId, Is.EqualTo(testUserId));
                Assert.That(viewModel.GameName, Is.EqualTo(string.Empty));
                Assert.That(viewModel.GameDescription, Is.EqualTo(string.Empty));
                Assert.That(viewModel.IsGameActive, Is.True);
                Assert.That(viewModel.GameImage, Is.Null);
            });
        }

        [Test]
        public void ValidateGameInputs_CoversValidAndInvalidScenarios()
        {
            PopulateWithValidInputs();
            Assert.That(viewModel.ValidateGameInputs(), Is.Empty);

            AssertValidationError(model => model.GameName = "AB", "Name");
            AssertValidationError(model => model.GameName = string.Empty, "Name");
            AssertValidationError(model => model.GamePrice = 0m, "Price");
            AssertValidationError(model => model.MinimumPlayersRequired = 0, "player");
            AssertValidationError(model =>
            {
                model.MinimumPlayersRequired = 5;
                model.MaximumPlayersAllowed = 2;
            }, "Maximum");
            AssertValidationError(model => model.GameDescription = "Short", "Description");

            PopulateWithValidInputs();
            viewModel.GameName = string.Empty;
            viewModel.GamePrice = 0m;
            viewModel.GameDescription = string.Empty;

            List<string> errors = viewModel.ValidateGameInputs();

            Assert.That(errors.Count, Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void PriceHelpers_ParseAndRoundTripValues()
        {
            viewModel.SetGamePriceFromText("25.50");
            Assert.That(viewModel.GamePrice, Is.EqualTo(25.50m));

            viewModel.GamePrice = 10m;
            viewModel.SetGamePriceFromText(string.Empty);
            Assert.That(viewModel.GamePrice, Is.EqualTo(0m));

            viewModel.GamePrice = 10m;
            viewModel.SetGamePriceFromText("not-a-price");
            Assert.That(viewModel.GamePrice, Is.EqualTo(0m));

            viewModel.GamePriceAsDouble = 19.99;

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.GamePrice, Is.EqualTo(19.99m));
                Assert.That(viewModel.GamePriceAsDouble, Is.EqualTo(19.99).Within(0.001));
            });
        }

        [Test]
        public async Task SubmitCreateGame_CoversSuccessAndValidationFailure()
        {
            PopulateWithValidInputs();

            ViewOperationResult successResult = await viewModel.SubmitCreateGameAsync();

            Assert.That(successResult.IsSuccess, Is.True);
            Assert.That(gameService.AddGameCallCount, Is.EqualTo(1));
            Assert.That(gameService.LastAddedGame!.Owner.Id, Is.EqualTo(testUserId));
            Assert.That(gameService.LastAddedGame.Name, Is.EqualTo("Settlers of Catan"));
            Assert.That(gameService.LastAddedGame.Price, Is.EqualTo(15.99m));

            gameService = new FakeClientGameService
            {
                ValidateGameHandler = gameService.ValidateGameHandler,
            };
            viewModel = new CreateGameViewModel(gameService, currentUserContext);
            PopulateWithValidInputs();
            viewModel.GameName = string.Empty;

            ViewOperationResult failureResult = await viewModel.SubmitCreateGameAsync();

            Assert.Multiple(() =>
            {
                Assert.That(failureResult.IsSuccess, Is.False);
                Assert.That(failureResult.DialogTitle, Is.EqualTo("Validation Error"));
            });
            Assert.That(gameService.AddGameCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task SaveGame_CoversSuccessAndValidationFailure()
        {
            PopulateWithValidInputs();

            GameDTO? savedGame = await viewModel.SaveGameAsync();
            GameDTO nonNullSavedGame = savedGame!;

            Assert.Multiple(() =>
            {
                Assert.That(savedGame, Is.Not.Null);
                Assert.That(nonNullSavedGame.Owner.Id, Is.EqualTo(testUserId));
                Assert.That(nonNullSavedGame.Name, Is.EqualTo("Settlers of Catan"));
                Assert.That(nonNullSavedGame.Price, Is.EqualTo(15.99m));
                Assert.That(nonNullSavedGame.MinimumPlayerNumber, Is.EqualTo(2));
                Assert.That(nonNullSavedGame.MaximumPlayerNumber, Is.EqualTo(6));
            });
            Assert.That(gameService.AddGameCallCount, Is.EqualTo(1));

            gameService = new FakeClientGameService
            {
                ValidateGameHandler = gameService.ValidateGameHandler,
            };
            viewModel = new CreateGameViewModel(gameService, currentUserContext);
            PopulateWithValidInputs();
            viewModel.GameName = string.Empty;

            GameDTO? invalidGame = await viewModel.SaveGameAsync();

            Assert.That(invalidGame, Is.Null);
            Assert.That(gameService.AddGameCallCount, Is.EqualTo(0));
        }

        private void AssertValidationError(Action<CreateGameViewModel> mutate, string expectedMessageFragment)
        {
            PopulateWithValidInputs();
            mutate(viewModel);

            List<string> errors = viewModel.ValidateGameInputs();

            Assert.That(errors, Has.Some.Contain(expectedMessageFragment));
        }

        private void PopulateWithValidInputs()
        {
            viewModel.GameName = "Settlers of Catan";
            viewModel.GamePrice = 15.99m;
            viewModel.MinimumPlayersRequired = 2;
            viewModel.MaximumPlayersAllowed = 6;
            viewModel.GameDescription = "A classic resource-trading board game for families.";
        }
    }
}
