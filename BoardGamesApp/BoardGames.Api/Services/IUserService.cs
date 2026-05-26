using System;
using System.Collections.Immutable;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IUserService
    {
        ImmutableList<UserDTO> GetUsersExcept(Guid excludeAccountId);
    }
}
