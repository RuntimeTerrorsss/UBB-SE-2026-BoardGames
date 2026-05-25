// <copyright file="UsersController2.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Api.Services;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;

        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet("except/{excludeAccountId:guid}")]
        public ActionResult<IReadOnlyList<UserDTO>> GetUsersExcept(Guid excludeAccountId)
        {
            return this.Ok(this.userService.GetUsersExcept(excludeAccountId));
        }
    }
}
