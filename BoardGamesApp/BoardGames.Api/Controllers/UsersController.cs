using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/legacy/users")]
    public class LegacyUsersController : ControllerBase
    {
        private readonly IUserRepository _repo;

        public LegacyUsersController(IUserRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _repo.GetById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            return Ok(await _repo.GetAll());
        }

        [HttpPut("{id}/address")]
        public async Task<ActionResult> SaveAddress(int id, [FromBody] Address address)
        {
            await _repo.SaveAddress(id, address);
            return NoContent();
        }

        [HttpGet("{id}/balance")]
        public async Task<ActionResult<decimal>> GetBalance(int id)
        {
            var user = await _repo.GetById(id);
            if (user == null) return NotFound();
            return Ok(user.Balance);
        }

        [HttpPut("{id}/balance")]
        public async Task<ActionResult> UpdateBalance(int id, [FromBody] decimal newBalance)
        {
            await _repo.UpdateBalance(id, newBalance);
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.EmailOrUsername) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest();
            }

            var user = await _repo.Login(request.EmailOrUsername, request.Password);
            if (user == null) return Unauthorized();

            return Ok(user);
        }

        public class LoginRequest
        {
            public string EmailOrUsername { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] User newUser)
        {
            var success = await _repo.Register(newUser);
            if (!success)
            {
                return BadRequest("Registration failed. Username/Email already exists.");
            }
            return Ok();
        }
    }
}
