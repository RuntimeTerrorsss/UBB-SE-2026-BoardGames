// <copyright file="IAdminService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Shared.ProxyServices
{
    public interface IAdminService
    {
        Task<ServiceResult<IReadOnlyList<AccountProfileDTO>>> GetAllAccountsAsync(int page, int pageSize, CancellationToken cancellationToken = default);

        Task<ServiceResult> SuspendAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

        Task<ServiceResult> UnsuspendAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

        Task<ServiceResult> ResetPasswordAsync(Guid accountId, string newPassword, CancellationToken cancellationToken = default);

        Task<ServiceResult> UnlockAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    }
}
