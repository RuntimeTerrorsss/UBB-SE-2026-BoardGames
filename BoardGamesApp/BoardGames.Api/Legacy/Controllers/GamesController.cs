using System.Collections.Generic;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Enum;
using BookingBoardGames.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly InterfaceGamesRepository gamesRepository;

        public GamesController(InterfaceGamesRepository gamesRepository)
        {
            this.gamesRepository = gamesRepository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Game>> GetGame(int id)
        {
            var game = await gamesRepository.GetGameById(id);
            if (game == null) return NotFound();
            return Ok(game);
        }

        [HttpGet]
        public async Task<ActionResult<List<Game>>> GetAll()
        {
            return await gamesRepository.GetAll();
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<Game>>> Filter([FromQuery] string? name)
        {
            var filter = new FilterCriteria { Name = name };
            return await gamesRepository.GetGamesByFilter(filter);
        }

        [HttpGet("{id}/price")]
        public async Task<ActionResult<decimal>> GetPrice(int id)
        {
            var price = await gamesRepository.GetPriceGameById(id);
            return Ok(price);
        }

        [HttpPost("search")]
        public async Task<ActionResult<List<Game>>> SearchGames([FromBody] FilterCriteria filter)
        {
            return await gamesRepository.GetGamesByFilter(filter);
        }

        [HttpGet("feed/tonight")]
        public async Task<ActionResult<List<Game>>> GetGamesFeedAvailableTonight([FromQuery] int userId)
        {
            return await gamesRepository.GetGamesForFeedAvailableTonight(userId);
        }

        [HttpGet("feed/remaining")]
        public async Task<ActionResult<List<Game>>> GetRemainingGamesForFeed([FromQuery] int userId)
        {
            return await gamesRepository.GetRemainingGamesForFeed(userId);
        }
    }
}
