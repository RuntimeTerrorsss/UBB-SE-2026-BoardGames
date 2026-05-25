// <copyright file="IAuthService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.Common;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<bool>> RegisterAsync(RegisterDTO dto);

        Task<ServiceResult<AccountProfileDTO>> LoginAsync(LoginDTO dto);

        Task<ServiceResult<bool>> LogoutAsync();

        Task<ServiceResult<string>> ForgotPasswordAsync();
    }
}
