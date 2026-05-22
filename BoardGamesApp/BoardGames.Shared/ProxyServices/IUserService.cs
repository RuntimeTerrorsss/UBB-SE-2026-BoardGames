using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardRentAndProperty.Contracts.DataTransferObjects;

namespace BoardGames.Shared.ProxyServices
{
    public interface IUserService
    {
        Task<ServiceResult<IReadOnlyList<UserDTO>>> GetUsersExceptAsync(Guid excludeAccountId, CancellationToken cancellationToken = default);
    }
}
