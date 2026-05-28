// <copyright file="CreateGameViewModelTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using NUnit.Framework;
using BoardGames.Desktop.ViewModels;
using BoardGames.Shared.DTO;
using BoardGames.Shared.Common;
using BoardGames.Data.Constants;

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
            this.gameService = new FakeClientGameService
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
            this.currentUserContext = new FakeCurrentUserContext { CurrentUserId = this.testUserId };

            this.viewModel = new Desktop.ViewModels.CreateGameViewModel(this.gameService, this.currentUserContext);
        }

        [Test]
        public void Constructor_InitializesCurrentUserAndDefaultState()
        {
            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.CurrentUserId, Is.EqualTo(this.testUserId));
                Assert.That(this.viewModel.GameName, Is.EqualTo(string.Empty));
                Assert.That(this.viewModel.GameDescription, Is.EqualTo(string.Empty));
                Assert.That(this.viewModel.IsGameActive, Is.True);
                Assert.That(this.viewModel.GameImage, Is.Null);
            });
        }

        [Test]
        public void ValidateGameInputs_CoversValidAndInvalidScenarios()
        {
            this.PopulateWithValidInputs();
            Assert.That(this.viewModel.ValidateGameInputs(), Is.Empty);

            this.AssertValidationError(model => model.GameName = "AB", "Name");
            this.AssertValidationError(model => model.GameName = string.Empty, "Name");
            this.AssertValidationError(model => model.GamePrice = 0m, "Price");
            this.AssertValidationError(model => model.MinimumPlayersRequired = 0, "player");
            this.AssertValidationError(
                model =>
            {
                model.MinimumPlayersRequired = 5;
                model.MaximumPlayersAllowed = 2;
            }, "Maximum");
            this.AssertValidationError(model => model.GameDescription = "Short", "Description");

            this.PopulateWithValidInputs();
            this.viewModel.GameName = string.Empty;
            this.viewModel.GamePrice = 0m;
            this.viewModel.GameDescription = string.Empty;

            List<string> errors = this.viewModel.ValidateGameInputs();

            Assert.That(errors.Count, Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void PriceHelpers_ParseAndRoundTripValues()
        {
            this.viewModel.SetGamePriceFromText("25.50");
            Assert.That(this.viewModel.GamePrice, Is.EqualTo(25.50m));

            this.viewModel.GamePrice = 10m;
            this.viewModel.SetGamePriceFromText(string.Empty);
            Assert.That(this.viewModel.GamePrice, Is.EqualTo(0m));

            this.viewModel.GamePrice = 10m;
            this.viewModel.SetGamePriceFromText("not-a-price");
            Assert.That(this.viewModel.GamePrice, Is.EqualTo(0m));

            this.viewModel.GamePriceAsDouble = 19.99;

            Assert.Multiple(() =>
            {
                Assert.That(this.viewModel.GamePrice, Is.EqualTo(19.99m));
                Assert.That(this.viewModel.GamePriceAsDouble, Is.EqualTo(19.99).Within(0.001));
            });
        }

        [Test]
        public async Task SubmitCreateGame_CoversSuccessAndValidationFailure()
        {
            this.PopulateWithValidInputs();

            ViewOperationResult successResult = await this.viewModel.SubmitCreateGameAsync();

            Assert.That(successResult.IsSuccess, Is.True);
            Assert.That(this.gameService.AddGameCallCount, Is.EqualTo(1));
            Assert.That(this.gameService.LastAddedGame!.OwnerId, Is.EqualTo(this.testUserId));
            Assert.That(this.gameService.LastAddedGame.Name, Is.EqualTo("Settlers of Catan"));
            Assert.That(this.gameService.LastAddedGame.Price, Is.EqualTo(15.99m));

            this.gameService = new FakeClientGameService
            {
                ValidateGameHandler = this.gameService.ValidateGameHandler,
            };
            this.viewModel = new CreateGameViewModel(this.gameService, this.currentUserContext);
            this.PopulateWithValidInputs();
            this.viewModel.GameName = string.Empty;

            ViewOperationResult failureResult = await this.viewModel.SubmitCreateGameAsync();

            Assert.Multiple(() =>
            {
                Assert.That(failureResult.IsSuccess, Is.False);
                Assert.That(failureResult.DialogTitle, Is.EqualTo("Validation Error"));
            });
            Assert.That(this.gameService.AddGameCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task SaveGame_CoversSuccessAndValidationFailure()
        {
            this.PopulateWithValidInputs();

            GameDTO? savedGame = await this.viewModel.SaveGameAsync();
            GameDTO nonNullSavedGame = savedGame!;

            Assert.Multiple(() =>
            {
                Assert.That(savedGame, Is.Not.Null);
                Assert.That(nonNullSavedGame.OwnerId, Is.EqualTo(this.testUserId));
                Assert.That(nonNullSavedGame.Name, Is.EqualTo("Settlers of Catan"));
                Assert.That(nonNullSavedGame.Price, Is.EqualTo(15.99m));
                Assert.That(nonNullSavedGame.MinimumPlayerNumber, Is.EqualTo(2));
                Assert.That(nonNullSavedGame.MaximumPlayerNumber, Is.EqualTo(6));
            });
            Assert.That(this.gameService.AddGameCallCount, Is.EqualTo(1));

            this.gameService = new FakeClientGameService
            {
                ValidateGameHandler = this.gameService.ValidateGameHandler,
            };
            this.viewModel = new CreateGameViewModel(this.gameService, this.currentUserContext);
            this.PopulateWithValidInputs();
            this.viewModel.GameName = string.Empty;

            GameDTO? invalidGame = await this.viewModel.SaveGameAsync();

            Assert.That(invalidGame, Is.Null);
            Assert.That(this.gameService.AddGameCallCount, Is.EqualTo(0));
        }

        private void AssertValidationError(Action<CreateGameViewModel> mutate, string expectedMessageFragment)
        {
            this.PopulateWithValidInputs();
            mutate(this.viewModel);

            List<string> errors = this.viewModel.ValidateGameInputs();

            Assert.That(errors, Has.Some.Contain(expectedMessageFragment));
        }

        private void PopulateWithValidInputs()
        {
            this.viewModel.GameName = "Settlers of Catan";
            this.viewModel.GamePrice = 15.99m;
            this.viewModel.MinimumPlayersRequired = 2;
            this.viewModel.MaximumPlayersAllowed = 6;
            this.viewModel.GameDescription = "A classic resource-trading board game for families.";
        }
    }
}
