

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Api.Services;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService gameService;

        public GamesController(IGameService gameService)
        {
            this.gameService = gameService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<GameSummaryDTO>>> GetAll()
        {
            var games = await this.gameService.GetAllActiveGames();
            return this.Ok(games);
        }

        [HttpGet("{gameId:int}")]
        public async Task<ActionResult<GameDetailDTO>> GetById(int gameId)
        {
            try
            {
                var game = await this.gameService.GetGameById(gameId);
                return this.Ok(game);
            }
            catch (KeyNotFoundException ex)
            {
                return this.NotFound(ex.Message);
            }
        }

        [HttpGet("{gameId:int}/image")]
        public async Task<IActionResult> GetImage(int gameId)
        {
            try
            {
                var imageBytes = await this.gameService.GetGameImage(gameId);
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    return this.NotFound("Image not found");
                }

                return this.File(imageBytes, "image/jpeg");
            }
            catch (KeyNotFoundException ex)
            {
                return this.NotFound(ex.Message);
            }
        }

        [HttpGet("renter/{renterAccountId:guid}/available")]
        public async Task<ActionResult<IReadOnlyList<GameSummaryDTO>>> GetAvailableForRenter(Guid renterAccountId)
        {
            var games = await this.gameService.GetAvailableGamesForRenter(renterAccountId);
            return this.Ok(games);
        }

        [HttpGet("owner/{ownerAccountId:guid}")]
        public ActionResult<IReadOnlyList<GameSummaryDTO>> GetByOwner(Guid ownerAccountId)
        {
            var games = this.gameService.GetGamesForOwner(ownerAccountId);
            return this.Ok(games);
        }

        [HttpGet("owner/{ownerAccountId:guid}/active")]
        public ActionResult<IReadOnlyList<GameSummaryDTO>> GetActiveByOwner(Guid ownerAccountId)
        {
            var games = this.gameService.GetActiveGamesForOwner(ownerAccountId);
            return this.Ok(games);
        }

        [HttpGet("admin")]
        public async Task<ActionResult<IReadOnlyList<GameSummaryDTO>>> GetAllGamesAdmin()
        {
            var games = await this.gameService.GetAllGamesAdmin();
            return this.Ok(games);
        }

        [HttpPost]
        public ActionResult<GameDetailDTO> Create([FromBody] GameCreateDTO body)
        {
            try
            {
                var game = this.gameService.CreateGame(body, body.OwnerAccountId);
                return this.CreatedAtAction(nameof(this.GetById), new { gameId = game.Id }, game);
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        [HttpPut("{gameId:int}")]
        public IActionResult Update(int gameId, [FromBody] GameUpdateDTO body, [FromQuery] Guid requestingAccountId, [FromQuery] bool isAdmin = false)
        {
            try
            {
                this.gameService.UpdateGame(gameId, body, requestingAccountId, isAdmin);
                return this.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return this.NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return this.Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        [HttpDelete("{gameId:int}")]
        public ActionResult<GameDetailDTO> Delete(int gameId, [FromQuery] Guid requestingAccountId, [FromQuery] bool isAdmin = false)
        {
            try
            {
                var deletedGame = this.gameService.DeleteGame(gameId, requestingAccountId, isAdmin);
                return this.Ok(deletedGame);
            }
            catch (KeyNotFoundException)
            {
                return this.NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return this.Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<IReadOnlyList<GameSummaryDTO>>> SearchGames([FromBody] GameSearchCriteriaDTO criteria)
        {
            var games = await this.gameService.SearchGames(criteria);
            return this.Ok(games);
        }
    }
}
