using System;
using System.Collections.Generic;
using BoardGames.Api.Services;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IBoardRentUserService userService;

        public UsersController(IBoardRentUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet("except/{excludeAccountId:guid}")]
        public ActionResult<IReadOnlyList<UserDTO>> GetUsersExcept(Guid excludeAccountId)
        {
            return Ok(userService.GetUsersExcept(excludeAccountId));
        }
    }
}
