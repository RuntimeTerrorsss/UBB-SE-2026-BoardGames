// <copyright file="UsersController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository repo;

        public UsersController(IUserRepository repository)
        {
            this.repo = repository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await this.repo.GetById(id);
            if (user == null)
            {
                return this.NotFound();
            }

            return this.Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            return this.Ok(await this.repo.GetAll());
        }

        [HttpPut("{id}/address")]
        public async Task<ActionResult> SaveAddress(int id, [FromBody] Address address)
        {
            await this.repo.SaveAddress(id, address);
            return this.NoContent();
        }

        [HttpGet("{id}/balance")]
        public async Task<ActionResult<decimal>> GetBalance(int id)
        {
            var user = await this.repo.GetById(id);
            if (user == null)
            {
                return this.NotFound();
            }

            return this.Ok(user.Balance);
        }

        [HttpPut("{id}/balance")]
        public async Task<ActionResult> UpdateBalance(int id, [FromBody] decimal newBalance)
        {
            await this.repo.UpdateBalance(id, newBalance);
            return this.NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.EmailOrUsername) || string.IsNullOrWhiteSpace(request.Password))
            {
                return this.BadRequest();
            }

            var user = await this.repo.Login(request.EmailOrUsername, request.Password);
            if (user == null)
            {
                return this.Unauthorized();
            }

            return this.Ok(user);
        }

        public class LoginRequest
        {
            public string EmailOrUsername { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] User newUser)
        {
            var success = await this.repo.Register(newUser);
            if (!success)
            {
                return this.BadRequest("Registration failed. Username/Email already exists.");
            }

            return this.Ok();
        }
    }
}
