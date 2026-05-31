using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Api.Mappers;
using BoardGames.Api.Services;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using Moq;
using NUnit.Framework;

namespace BoardGames.Tests.Api.Services
{
    [TestFixture]
    public class GameServiceTests
    {
        private Mock<InterfaceGamesRepository> mockGameRepository;
        private Mock<IRentalRepository> mockRentalRepository;
        private Mock<IRequestService> mockRequestService;
        private GameService gameService;

        [SetUp]
        public void Setup()
        {
            mockGameRepository = new Mock<InterfaceGamesRepository>();
            mockRentalRepository = new Mock<IRentalRepository>();
            mockRequestService = new Mock<IRequestService>();

            var gameMapper = new GameMapper(new UserMapper());

            gameService = new GameService(
                mockGameRepository.Object,
                mockRentalRepository.Object,
                gameMapper,
                mockRequestService.Object);
        }

        [Test]
        public async Task GetAllActiveGames_GamesExist_ReturnsSummaryList()
        {
            var games = new List<Game> { new Game { Id = 1, Name = "Game1", IsActive = true } };
            mockGameRepository.Setup(r => r.GetAll()).ReturnsAsync(games);

            var result = await gameService.GetAllActiveGames();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Game1"));
        }

        [Test]
        public async Task GetAllActiveGames_NoGames_ReturnsEmptyList()
        {
            mockGameRepository.Setup(r => r.GetAll()).ReturnsAsync(new List<Game>());

            var result = await gameService.GetAllActiveGames();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetGamesForOwner_ValidOwner_ReturnsSummaryList()
        {
            var ownerId = Guid.NewGuid();
            var games = new List<Game> { new Game { Id = 1, Name = "Game1", Owner = new User { Id = ownerId } } };
            mockGameRepository.Setup(r => r.GetGamesByOwner(ownerId)).Returns(games);

            var result = gameService.GetGamesForOwner(ownerId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetGamesForOwner_OwnerWithNoGames_ReturnsEmptyList()
        {
            var ownerId = Guid.NewGuid();
            mockGameRepository.Setup(r => r.GetGamesByOwner(ownerId)).Returns(new List<Game>());

            var result = gameService.GetGamesForOwner(ownerId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetActiveGamesForOwner_ValidOwner_ReturnsActiveOnly()
        {
            var ownerId = Guid.NewGuid();
            var games = new List<Game>
            {
                new Game { Id = 1, IsActive = true, Owner = new User { Id = ownerId } },
                new Game { Id = 2, IsActive = false, Owner = new User { Id = ownerId } }
            };
            mockGameRepository.Setup(r => r.GetGamesByOwner(ownerId)).Returns(games);

            var result = gameService.GetActiveGamesForOwner(ownerId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllGamesAdmin_GamesExist_ReturnsAllGamesIncludingInactive()
        {
            var games = new List<Game> { new Game { Id = 1, IsActive = false } };
            mockGameRepository.Setup(r => r.GetAllIncludingInactive()).ReturnsAsync(games);

            var result = await gameService.GetAllGamesAdmin();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetGameById_ExistingGame_ReturnsDetailDTO()
        {
            var game = new Game { Id = 1, Name = "Game1" };
            mockGameRepository.Setup(r => r.GetGameById(1)).ReturnsAsync(game);

            var result = await gameService.GetGameById(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
        }

        [Test]
        public void GetGameById_NonExistingGame_ThrowsKeyNotFoundException()
        {
            mockGameRepository.Setup(r => r.GetGameById(99)).ReturnsAsync((Game)null);

            Assert.ThrowsAsync<KeyNotFoundException>(() => gameService.GetGameById(99));
        }

        [Test]
        public async Task GetGameImage_ExistingGame_ReturnsImageBytes()
        {
            var imageBytes = new byte[] { 1, 2, 3 };
            var game = new Game { Id = 1, Image = imageBytes };
            mockGameRepository.Setup(r => r.GetGameById(1)).ReturnsAsync(game);

            var result = await gameService.GetGameImage(1);

            Assert.That(result, Is.EqualTo(imageBytes));
        }

        [Test]
        public void GetGameImage_NonExistingGame_ThrowsKeyNotFoundException()
        {
            mockGameRepository.Setup(r => r.GetGameById(99)).ReturnsAsync((Game)null);

            Assert.ThrowsAsync<KeyNotFoundException>(() => gameService.GetGameImage(99));
        }

        [Test]
        public void CreateGame_ValidData_ReturnsDetailDTO()
        {
            var ownerId = Guid.NewGuid();
            var dto = new GameCreateDTO
            {
                Name = "ValidName",
                Price = 10,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
                Description = new string('A', 20)
            };

            var result = gameService.CreateGame(dto, ownerId);

            mockGameRepository.Verify(r => r.AddGame(It.IsAny<Game>()), Times.Once);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("ValidName"));
        }

        [Test]
        public void CreateGame_InvalidData_ThrowsArgumentException()
        {
            var ownerId = Guid.NewGuid();
            var dto = new GameCreateDTO
            {
                Name = "",
                Price = -5,
                MinimumPlayerNumber = 0,
                MaximumPlayerNumber = 0,
                Description = ""
            };

            Assert.Throws<ArgumentException>(() => gameService.CreateGame(dto, ownerId));
        }

        [Test]
        public void UpdateGame_ValidDataAndOwner_UpdatesSuccessfully()
        {
            var ownerId = Guid.NewGuid();
            var game = new Game { Id = 1, Owner = new User { Id = ownerId } };
            mockGameRepository.Setup(r => r.GetGame(1)).Returns(game);

            var dto = new GameUpdateDTO
            {
                Name = "UpdatedName",
                Price = 15,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
                Description = new string('A', 20)
            };

            gameService.UpdateGame(1, dto, ownerId, false);

            mockGameRepository.Verify(r => r.UpdateGame(1, It.IsAny<Game>()), Times.Once);
        }

        [Test]
        public void UpdateGame_InvalidData_ThrowsArgumentException()
        {
            var ownerId = Guid.NewGuid();
            var game = new Game { Id = 1, Owner = new User { Id = ownerId } };
            mockGameRepository.Setup(r => r.GetGame(1)).Returns(game);

            var dto = new GameUpdateDTO { Name = "" };

            Assert.Throws<ArgumentException>(() => gameService.UpdateGame(1, dto, ownerId, false));
        }

        [Test]
        public void UpdateGame_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            var ownerId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            var game = new Game { Id = 1, Owner = new User { Id = ownerId } };
            mockGameRepository.Setup(r => r.GetGame(1)).Returns(game);

            var dto = new GameUpdateDTO
            {
                Name = "UpdatedName",
                Price = 15,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
                Description = new string('A', 20)
            };

            Assert.Throws<UnauthorizedAccessException>(() => gameService.UpdateGame(1, dto, requesterId, false));
        }

        [Test]
        public void UpdateGame_AdminUser_UpdatesSuccessfully()
        {
            var ownerId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var game = new Game { Id = 1, Owner = new User { Id = ownerId } };
            mockGameRepository.Setup(r => r.GetGame(1)).Returns(game);

            var dto = new GameUpdateDTO
            {
                Name = "UpdatedName",
                Price = 15,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
                Description = new string('A', 20)
            };

            gameService.UpdateGame(1, dto, adminId, true);

            mockGameRepository.Verify(r => r.UpdateGame(1, It.IsAny<Game>()), Times.Once);
        }

        [Test]
        public void DeleteGame_OwnerNoActiveRentals_DeletesSuccessfully()
        {
            var ownerId = Guid.NewGuid();
            var game = new Game { Id = 1, Owner = new User { Id = ownerId } };
            mockGameRepository.Setup(r => r.GetGame(1)).Returns(game);
            mockRentalRepository.Setup(r => r.GetRentalsByGame(1)).Returns(new List<Rental>());

            var result = gameService.DeleteGame(1, ownerId, false);

            mockGameRepository.Verify(r => r.DeleteGame(1), Times.Once);
            mockRequestService.Verify(r => r.OnGameDeactivated(1), Times.Once);
            Assert.That(result.Id, Is.EqualTo(1));
        }

        [Test]
        public void DeleteGame_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            var ownerId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            var game = new Game { Id = 1, Owner = new User { Id = ownerId } };
            mockGameRepository.Setup(r => r.GetGame(1)).Returns(game);

            Assert.Throws<UnauthorizedAccessException>(() => gameService.DeleteGame(1, requesterId, false));
        }

        [Test]
        public void DeleteGame_ActiveRentals_ThrowsInvalidOperationException()
        {
            var ownerId = Guid.NewGuid();
            var game = new Game { Id = 1, Owner = new User { Id = ownerId } };
            mockGameRepository.Setup(r => r.GetGame(1)).Returns(game);

            var activeRental = new Rental { EndDate = DateTime.Now.AddDays(1) };
            mockRentalRepository.Setup(r => r.GetRentalsByGame(1)).Returns(new List<Rental> { activeRental });

            Assert.Throws<InvalidOperationException>(() => gameService.DeleteGame(1, ownerId, false));
        }

        [Test]
        public void DeleteGame_AdminUser_DeletesSuccessfully()
        {
            var ownerId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var game = new Game { Id = 1, Owner = new User { Id = ownerId } };
            mockGameRepository.Setup(r => r.GetGame(1)).Returns(game);
            mockRentalRepository.Setup(r => r.GetRentalsByGame(1)).Returns(new List<Rental>());

            gameService.DeleteGame(1, adminId, true);

            mockGameRepository.Verify(r => r.DeleteGame(1), Times.Once);
        }

        [Test]
        public async Task SearchGames_ValidCriteria_ReturnsMatches()
        {
            var criteria = new GameSearchCriteriaDTO { Name = "SearchTerm" };
            var games = new List<Game> { new Game { Id = 1, Name = "SearchTerm Game" } };
            mockGameRepository.Setup(r => r.GetGamesByFilter(It.IsAny<FilterCriteria>())).ReturnsAsync(games);

            var result = await gameService.SearchGames(criteria);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("SearchTerm Game"));
        }
        
        [Test]
        public async Task SearchGames_NoMatches_ReturnsEmptyList()
        {
            var criteria = new GameSearchCriteriaDTO { Name = "NoMatch" };
            mockGameRepository.Setup(r => r.GetGamesByFilter(It.IsAny<FilterCriteria>())).ReturnsAsync(new List<Game>());

            var result = await gameService.SearchGames(criteria);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }
    }
}
