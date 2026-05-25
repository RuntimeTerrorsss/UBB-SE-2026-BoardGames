using BoardGames.Shared.DTO;
using System.Collections.Immutable;

namespace BoardGames.Api.Services
{
    public interface IUserService
    {
        ImmutableList<UserDTO> GetUsersExcept(Guid excludeAccountId);
    }
}
