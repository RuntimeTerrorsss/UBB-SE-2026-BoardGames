using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Api.Controllers;
using BoardGames.Api.Services;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace BoardGames.Tests.Api.Controllers
{
    [TestFixture]
    public class GamesControllerTests
    {
        private Mock<IGameService> mockGameService;
        private GamesController controller;

        [SetUp]
        public void Setup()
        {
            mockGameService = new Mock<IGameService>();
            controller = new GamesController(mockGameService.Object);
        }

        [Test]
        public async Task GetAll_ActiveGamesExist_ReturnsOkWithGames()
        {
            var expectedGames = new List<GameSummaryDTO> { new GameSummaryDTO { Id = 1, Name = "Game 1" } };
            mockGameService.Setup(s => s.GetAllActiveGames()).ReturnsAsync(expectedGames);

            var result = await controller.GetAll();

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedGames));
        }

        [Test]
        public async Task GetAll_NoActiveGames_ReturnsOkWithEmptyList()
        {
            var expectedGames = new List<GameSummaryDTO>();
            mockGameService.Setup(s => s.GetAllActiveGames()).ReturnsAsync(expectedGames);

            var result = await controller.GetAll();

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedGames));
        }

        [Test]
        public async Task GetById_ExistingId_ReturnsOkWithGame()
        {
            var expectedGame = new GameDetailDTO { Id = 1, Name = "Test Game" };
            mockGameService.Setup(s => s.GetGameById(1)).ReturnsAsync(expectedGame);

            var result = await controller.GetById(1);

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedGame));
        }

        [Test]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            mockGameService.Setup(s => s.GetGameById(99)).ThrowsAsync(new KeyNotFoundException("Not found"));

            var result = await controller.GetById(99);

            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult.Value, Is.EqualTo("Not found"));
        }

        [Test]
        public async Task GetImage_ExistingIdWithImage_ReturnsFileResult()
        {
            var imageBytes = new byte[] { 1, 2, 3 };
            mockGameService.Setup(s => s.GetGameImage(1)).ReturnsAsync(imageBytes);

            var result = await controller.GetImage(1);

            var fileResult = result as FileContentResult;
            Assert.That(fileResult, Is.Not.Null);
            Assert.That(fileResult.FileContents, Is.EqualTo(imageBytes));
            Assert.That(fileResult.ContentType, Is.EqualTo("image/jpeg"));
        }

        [Test]
        public async Task GetImage_ExistingIdNoImage_ReturnsNotFound()
        {
            mockGameService.Setup(s => s.GetGameImage(1)).ReturnsAsync(Array.Empty<byte>());

            var result = await controller.GetImage(1);

            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult.Value, Is.EqualTo("Image not found"));
        }

        [Test]
        public async Task GetImage_ExistingIdNullImage_ReturnsNotFound()
        {
            mockGameService.Setup(s => s.GetGameImage(1)).ReturnsAsync((byte[])null);

            var result = await controller.GetImage(1);

            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult.Value, Is.EqualTo("Image not found"));
        }

        [Test]
        public async Task GetImage_NonExistingId_ReturnsNotFound()
        {
            mockGameService.Setup(s => s.GetGameImage(99)).ThrowsAsync(new KeyNotFoundException("Not found"));

            var result = await controller.GetImage(99);

            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult.Value, Is.EqualTo("Not found"));
        }

        [Test]
        public void GetByOwner_ValidOwnerId_ReturnsOkWithGames()
        {
            var ownerId = Guid.NewGuid();
            var expectedGames = new List<GameSummaryDTO> { new GameSummaryDTO { Id = 1, Name = "Game 1" } };
            mockGameService.Setup(s => s.GetGamesForOwner(ownerId)).Returns(expectedGames);

            var result = controller.GetByOwner(ownerId);

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedGames));
        }

        [Test]
        public void GetByOwner_OwnerWithNoGames_ReturnsOkWithEmptyList()
        {
            var ownerId = Guid.NewGuid();
            var expectedGames = new List<GameSummaryDTO>();
            mockGameService.Setup(s => s.GetGamesForOwner(ownerId)).Returns(expectedGames);

            var result = controller.GetByOwner(ownerId);

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedGames));
        }

        [Test]
        public void GetActiveByOwner_ValidOwnerId_ReturnsOkWithActiveGames()
        {
            var ownerId = Guid.NewGuid();
            var expectedGames = new List<GameSummaryDTO> { new GameSummaryDTO { Id = 1, Name = "Game 1" } };
            mockGameService.Setup(s => s.GetActiveGamesForOwner(ownerId)).Returns(expectedGames);

            var result = controller.GetActiveByOwner(ownerId);

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedGames));
        }

        [Test]
        public void GetActiveByOwner_OwnerWithNoActiveGames_ReturnsOkWithEmptyList()
        {
            var ownerId = Guid.NewGuid();
            var expectedGames = new List<GameSummaryDTO>();
            mockGameService.Setup(s => s.GetActiveGamesForOwner(ownerId)).Returns(expectedGames);

            var result = controller.GetActiveByOwner(ownerId);

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedGames));
        }

        [Test]
        public async Task GetAllGamesAdmin_AdminUser_ReturnsOkWithAllGames()
        {
            var expectedGames = new List<GameSummaryDTO> { new GameSummaryDTO { Id = 1, Name = "Game 1" } };
            mockGameService.Setup(s => s.GetAllGamesAdmin()).ReturnsAsync(expectedGames);

            var result = await controller.GetAllGamesAdmin();

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedGames));
        }

        [Test]
        public void Create_ValidInput_ReturnsCreatedAtAction()
        {
            var ownerId = Guid.NewGuid();
            var createDto = new GameCreateDTO { Name = "New Game", OwnerAccountId = ownerId };
            var expectedGame = new GameDetailDTO { Id = 1, Name = "New Game" };
            mockGameService.Setup(s => s.CreateGame(createDto, ownerId)).Returns(expectedGame);

            var result = controller.Create(createDto);

            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult.ActionName, Is.EqualTo(nameof(controller.GetById)));
            Assert.That(createdResult.RouteValues["gameId"], Is.EqualTo(1));
            Assert.That(createdResult.Value, Is.EqualTo(expectedGame));
        }

        [Test]
        public void Create_InvalidInput_ReturnsBadRequest()
        {
            var createDto = new GameCreateDTO { Name = "" };
            mockGameService.Setup(s => s.CreateGame(createDto, It.IsAny<Guid>())).Throws(new ArgumentException("Invalid name"));

            var result = controller.Create(createDto);

            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid name"));
        }

        [Test]
        public void Update_ValidInput_ReturnsNoContent()
        {
            var updateDto = new GameUpdateDTO { Name = "Updated Game" };
            var requestingAccountId = Guid.NewGuid();
            mockGameService.Setup(s => s.UpdateGame(1, updateDto, requestingAccountId, false));

            var result = controller.Update(1, updateDto, requestingAccountId, false);

            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public void Update_NonExistingId_ReturnsNotFound()
        {
            var updateDto = new GameUpdateDTO { Name = "Updated Game" };
            var requestingAccountId = Guid.NewGuid();
            mockGameService.Setup(s => s.UpdateGame(99, updateDto, requestingAccountId, false)).Throws(new KeyNotFoundException());

            var result = controller.Update(99, updateDto, requestingAccountId, false);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public void Update_UnauthorizedUser_ReturnsForbid()
        {
            var updateDto = new GameUpdateDTO { Name = "Updated Game" };
            var requestingAccountId = Guid.NewGuid();
            mockGameService.Setup(s => s.UpdateGame(1, updateDto, requestingAccountId, false)).Throws(new UnauthorizedAccessException("Not authorized"));

            var result = controller.Update(1, updateDto, requestingAccountId, false);

            var forbidResult = result as ForbidResult;
            Assert.That(forbidResult, Is.Not.Null);
        }

        [Test]
        public void Update_InvalidInput_ReturnsBadRequest()
        {
            var updateDto = new GameUpdateDTO { Name = "" };
            var requestingAccountId = Guid.NewGuid();
            mockGameService.Setup(s => s.UpdateGame(1, updateDto, requestingAccountId, false)).Throws(new ArgumentException("Invalid input"));

            var result = controller.Update(1, updateDto, requestingAccountId, false);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid input"));
        }

        [Test]
        public void Delete_ExistingIdAuthorizedUser_ReturnsOkWithGame()
        {
            var requestingAccountId = Guid.NewGuid();
            var expectedGame = new GameDetailDTO { Id = 1, Name = "Deleted Game" };
            mockGameService.Setup(s => s.DeleteGame(1, requestingAccountId, false)).Returns(expectedGame);

            var result = controller.Delete(1, requestingAccountId, false);

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedGame));
        }

        [Test]
        public void Delete_NonExistingId_ReturnsNotFound()
        {
            var requestingAccountId = Guid.NewGuid();
            mockGameService.Setup(s => s.DeleteGame(99, requestingAccountId, false)).Throws(new KeyNotFoundException());

            var result = controller.Delete(99, requestingAccountId, false);

            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public void Delete_UnauthorizedUser_ReturnsForbid()
        {
            var requestingAccountId = Guid.NewGuid();
            mockGameService.Setup(s => s.DeleteGame(1, requestingAccountId, false)).Throws(new UnauthorizedAccessException("Not authorized"));

            var result = controller.Delete(1, requestingAccountId, false);

            var forbidResult = result.Result as ForbidResult;
            Assert.That(forbidResult, Is.Not.Null);
        }

        [Test]
        public void Delete_GameWithActiveRentals_ReturnsBadRequest()
        {
            var requestingAccountId = Guid.NewGuid();
            mockGameService.Setup(s => s.DeleteGame(1, requestingAccountId, false)).Throws(new InvalidOperationException("Active rentals exist"));

            var result = controller.Delete(1, requestingAccountId, false);

            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.Value, Is.EqualTo("Active rentals exist"));
        }

        [Test]
        public async Task SearchGames_ValidCriteria_ReturnsOkWithGames()
        {
            var criteria = new GameSearchCriteriaDTO { Name = "SearchTerm" };
            var expectedGames = new List<GameSummaryDTO> { new GameSummaryDTO { Id = 1, Name = "SearchTerm Game" } };
            mockGameService.Setup(s => s.SearchGames(criteria)).ReturnsAsync(expectedGames);

            var result = await controller.SearchGames(criteria);

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedGames));
        }

        [Test]
        public async Task SearchGames_NoMatches_ReturnsOkWithEmptyList()
        {
            var criteria = new GameSearchCriteriaDTO { Name = "NoMatch" };
            var expectedGames = new List<GameSummaryDTO>();
            mockGameService.Setup(s => s.SearchGames(criteria)).ReturnsAsync(expectedGames);

            var result = await controller.SearchGames(criteria);

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(expectedGames));
        }
    }
}
