// <copyright file="IAuthService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Shared.ProxyServices
{
    public interface IAuthService
    {
        Task<ServiceResult> RegisterAsync(RegisterDTO request, CancellationToken cancellationToken = default);

        Task<ServiceResult<AccountProfileDTO>> LoginAsync(LoginDTO request, CancellationToken cancellationToken = default);

        Task<ServiceResult> LogoutAsync(CancellationToken cancellationToken = default);

        Task<ServiceResult<string>> ForgotPasswordAsync(CancellationToken cancellationToken = default);
    }
}
