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
            var games = await gameService.GetAllActiveGames();
            return Ok(games);
        }

        [HttpGet("{gameId:int}")]
        public async Task<ActionResult<GameDetailDTO>> GetById(int gameId)
        {
            try
            {
                var game = await gameService.GetGameById(gameId);
                return Ok(game);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{gameId:int}/image")]
        public async Task<IActionResult> GetImage(int gameId)
        {
            try
            {
                var imageBytes = await gameService.GetGameImage(gameId);
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    return NotFound("Image not found");
                }
                return File(imageBytes, "image/jpeg");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("owner/{ownerAccountId:guid}")]
        public ActionResult<IReadOnlyList<GameSummaryDTO>> GetByOwner(Guid ownerAccountId)
        {
            var games = gameService.GetGamesForOwner(ownerAccountId);
            return Ok(games);
        }

        [HttpGet("owner/{ownerAccountId:guid}/active")]
        public ActionResult<IReadOnlyList<GameSummaryDTO>> GetActiveByOwner(Guid ownerAccountId)
        {
            var games = gameService.GetActiveGamesForOwner(ownerAccountId);
            return Ok(games);
        }

        [HttpGet("admin")]
        public async Task<ActionResult<IReadOnlyList<GameSummaryDTO>>> GetAllGamesAdmin()
        {
            // Note: Authorization is enforced by the caller/middleware (Admin role required)
            var games = await gameService.GetAllGamesAdmin();
            return Ok(games);
        }

        [HttpPost]
        public ActionResult<GameDetailDTO> Create([FromBody] GameCreateDTO body)
        {
            try
            {
                // OwnerAccountId is temporarily sourced from the body until Task 7 wires up session identity.
                var game = gameService.CreateGame(body, body.OwnerAccountId);
                return CreatedAtAction(nameof(GetById), new { gameId = game.Id }, game);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{gameId:int}")]
        public IActionResult Update(int gameId, [FromBody] GameUpdateDTO body, [FromQuery] Guid requestingAccountId, [FromQuery] bool isAdmin = false)
        {
            try
            {
                // requestingAccountId & isAdmin are temporarily sourced from query params until Task 7 wires up session identity.
                gameService.UpdateGame(gameId, body, requestingAccountId, isAdmin);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{gameId:int}")]
        public ActionResult<GameDetailDTO> Delete(int gameId, [FromQuery] Guid requestingAccountId, [FromQuery] bool isAdmin = false)
        {
            try
            {
                // requestingAccountId & isAdmin are temporarily sourced from query params until Task 7 wires up session identity.
                var deletedGame = gameService.DeleteGame(gameId, requestingAccountId, isAdmin);
                return Ok(deletedGame);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<IReadOnlyList<GameSummaryDTO>>> SearchGames([FromBody] GameSearchCriteriaDTO criteria)
        {
            var games = await gameService.SearchGames(criteria);
            return Ok(games);
        }
    }
}
