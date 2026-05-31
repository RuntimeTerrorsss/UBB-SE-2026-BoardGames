//// <copyright file="SearchGamesViewModelTests.cs" company="BoardRent">
//// Copyright (c) BoardRent. All rights reserved.
//// </copyright>

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using BoardGames.Desktop.Services;
//using BoardGames.Desktop.ViewModels;
//using BoardGames.Shared.DTO;
//using BoardGames.Shared.ProxyServices;
//using BoardGames.Tests.Fakes;
//using Microsoft.UI.Xaml;
//using NUnit.Framework;

//namespace BoardGames.Tests.ViewModels
//{
//    [TestFixture]
//    public sealed class SearchGamesViewModelTests
//    {
//        private static readonly Uri ApiBaseUri = new("http://localhost:5018/");
//        private FakeClientGameService gameService = null!;
//        private FakeSessionContext sessionContext = null!;
//        private SearchGamesViewModel systemUnderTest = null!;

//        [SetUp]
//        public void SetUp()
//        {
//            this.gameService = new FakeClientGameService();
//            this.sessionContext = new FakeSessionContext();
//            this.systemUnderTest = new SearchGamesViewModel(this.gameService, this.sessionContext, ApiBaseUri);
//        }

//        [Test]
//        public void LoginButtonVisibility_WhenAnonymous_IsVisible()
//        {
//            Assert.That(this.systemUnderTest.LoginButtonVisibility, Is.EqualTo(Visibility.Visible));
//        }

//        [Test]
//        public async Task LoadAsync_WithNoFilters_CallsGetAllGames()
//        {
//            this.gameService.AllGamesResult = ServiceResult<IReadOnlyList<GameSummaryDTO>>.Ok(new[]
//            {
//                BuildGameSummary(1, "Catan"),
//                BuildGameSummary(2, "Carcassonne"),
//            });

//            await this.systemUnderTest.LoadAsync();

//            Assert.That(this.gameService.GetAllGamesCallCount, Is.EqualTo(1));
//            Assert.That(this.gameService.SearchGamesCallCount, Is.EqualTo(0));
//            Assert.That(this.systemUnderTest.Games, Has.Count.EqualTo(2));
//        }

//        [Test]
//        public async Task SearchAsync_WithFilters_CallsSearchGamesWithMappedCriteria()
//        {
//            this.systemUnderTest.SearchText = "  Catan  ";
//            this.systemUnderTest.City = "  Cluj-Napoca  ";
//            this.systemUnderTest.MaximumPrice = 19.99;
//            this.systemUnderTest.PlayerCount = 3.2;
//            this.systemUnderTest.AvailableFrom = new DateTimeOffset(2026, 6, 10, 0, 0, 0, TimeSpan.Zero);
//            this.systemUnderTest.AvailableTo = new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero);
//            this.systemUnderTest.SelectedSortOption = "PriceDescending";

//            await this.systemUnderTest.SearchCommand.ExecuteAsync(null);

//            Assert.That(this.gameService.GetAllGamesCallCount, Is.EqualTo(0));
//            Assert.That(this.gameService.SearchGamesCallCount, Is.EqualTo(1));
//            Assert.That(this.gameService.LastSearchCriteria, Is.Not.Null);
//            Assert.That(this.gameService.LastSearchCriteria!.Name, Is.EqualTo("Catan"));
//            Assert.That(this.gameService.LastSearchCriteria.City, Is.EqualTo("Cluj-Napoca"));
//            Assert.That(this.gameService.LastSearchCriteria.MaximumPrice, Is.EqualTo(19.99m));
//            Assert.That(this.gameService.LastSearchCriteria.PlayerCount, Is.EqualTo(4));
//            Assert.That(this.gameService.LastSearchCriteria.AvailableFrom, Is.EqualTo(new DateTime(2026, 6, 10)));
//            Assert.That(this.gameService.LastSearchCriteria.AvailableTo, Is.EqualTo(new DateTime(2026, 6, 12)));
//            Assert.That(this.gameService.LastSearchCriteria.SortBy, Is.EqualTo("PriceDescending"));
//        }

//        [Test]
//        public async Task SearchAsync_WithInvalidDateRange_ShowsErrorWithoutCallingService()
//        {
//            this.systemUnderTest.AvailableFrom = new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero);
//            this.systemUnderTest.AvailableTo = new DateTimeOffset(2026, 6, 10, 0, 0, 0, TimeSpan.Zero);

//            await this.systemUnderTest.SearchCommand.ExecuteAsync(null);

//            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("End date cannot be earlier than the start date."));
//            Assert.That(this.gameService.GetAllGamesCallCount, Is.EqualTo(0));
//            Assert.That(this.gameService.SearchGamesCallCount, Is.EqualTo(0));
//        }

//        [Test]
//        public async Task LoadAsync_WhenServiceFails_ClearsGamesAndShowsError()
//        {
//            this.gameService.AllGamesResult = ServiceResult<IReadOnlyList<GameSummaryDTO>>.Fail("Backend unavailable.");

//            await this.systemUnderTest.LoadAsync();

//            Assert.That(this.systemUnderTest.Games, Is.Empty);
//            Assert.That(this.systemUnderTest.ResultsSummary, Is.EqualTo(string.Empty));
//            Assert.That(this.systemUnderTest.EmptyStateMessage, Is.EqualTo(string.Empty));
//            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo("Backend unavailable."));
//        }

//        [Test]
//        public async Task LoadAsync_WithSuccessfulResponse_FillsGamesAndSummary()
//        {
//            this.gameService.AllGamesResult = ServiceResult<IReadOnlyList<GameSummaryDTO>>.Ok(new[]
//            {
//                BuildGameSummary(1, "Catan"),
//                BuildGameSummary(2, "Azul"),
//            });

//            await this.systemUnderTest.LoadAsync();

//            Assert.That(this.systemUnderTest.Games, Has.Count.EqualTo(2));
//            Assert.That(this.systemUnderTest.Games.Select(game => game.Name), Is.EqualTo(new[] { "Catan", "Azul" }));
//            Assert.That(this.systemUnderTest.ResultsSummary, Is.EqualTo("2 games available"));
//            Assert.That(this.systemUnderTest.EmptyStateMessage, Is.EqualTo(string.Empty));
//            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo(string.Empty));
//        }

//        [Test]
//        public async Task LoadAsync_WithNoGames_ShowsEmptyStateMessage()
//        {
//            this.gameService.AllGamesResult = ServiceResult<IReadOnlyList<GameSummaryDTO>>.Ok(Array.Empty<GameSummaryDTO>());

//            await this.systemUnderTest.LoadAsync();

//            Assert.That(this.systemUnderTest.ResultsSummary, Is.EqualTo("No games matched the current search."));
//            Assert.That(this.systemUnderTest.EmptyStateMessage, Is.EqualTo("Try clearing a filter or broadening your search."));
//        }

//        [Test]
//        public async Task ClearAsync_ResetsFiltersAndReloadsAllGames()
//        {
//            this.gameService.AllGamesResult = ServiceResult<IReadOnlyList<GameSummaryDTO>>.Ok(new[]
//            {
//                BuildGameSummary(1, "Wingspan"),
//            });
//            this.systemUnderTest.SearchText = "Wings";
//            this.systemUnderTest.City = "Brasov";
//            this.systemUnderTest.MaximumPrice = 25;
//            this.systemUnderTest.PlayerCount = 3;
//            this.systemUnderTest.AvailableFrom = new DateTimeOffset(2026, 6, 10, 0, 0, 0, TimeSpan.Zero);
//            this.systemUnderTest.AvailableTo = new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero);
//            this.systemUnderTest.SelectedSortOption = "PriceAscending";
//            this.systemUnderTest.ErrorMessage = "Previous error";

//            await this.systemUnderTest.ClearCommand.ExecuteAsync(null);

//            Assert.That(this.systemUnderTest.SearchText, Is.EqualTo(string.Empty));
//            Assert.That(this.systemUnderTest.City, Is.EqualTo(string.Empty));
//            Assert.That(this.systemUnderTest.MaximumPrice, Is.EqualTo(0));
//            Assert.That(this.systemUnderTest.PlayerCount, Is.EqualTo(0));
//            Assert.That(this.systemUnderTest.AvailableFrom, Is.Null);
//            Assert.That(this.systemUnderTest.AvailableTo, Is.Null);
//            Assert.That(this.systemUnderTest.SelectedSortOption, Is.EqualTo("None"));
//            Assert.That(this.systemUnderTest.ErrorMessage, Is.EqualTo(string.Empty));
//            Assert.That(this.gameService.GetAllGamesCallCount, Is.EqualTo(1));
//        }

//        [Test]
//        public void NavigateToLogin_WhenExecuted_InvokesCallback()
//        {
//            bool navigateToLoginWasCalled = false;
//            this.systemUnderTest.OnNavigateToLogin = () => navigateToLoginWasCalled = true;

//            this.systemUnderTest.NavigateToLoginCommand.Execute(null);

//            Assert.That(navigateToLoginWasCalled, Is.True);
//        }

//        [Test]
//        public void LoginButtonVisibility_WhenLoggedIn_IsCollapsed()
//        {
//            this.sessionContext.IsLoggedIn = true;
//            this.systemUnderTest = new SearchGamesViewModel(this.gameService, this.sessionContext, ApiBaseUri);

//            Assert.That(this.systemUnderTest.LoginButtonVisibility, Is.EqualTo(Visibility.Collapsed));
//        }

//        private static GameSummaryDTO BuildGameSummary(int id, string name)
//        {
//            return new GameSummaryDTO
//            {
//                Id = id,
//                Name = name,
//                Price = 18.5m,
//                City = "Cluj-Napoca",
//                MinimumPlayerNumber = 2,
//                MaximumPlayerNumber = 4,
//                OwnerDisplayName = "Owner",
//                ImageUrl = string.Empty,
//                IsActive = true,
//            };
//        }
//    }
//}
