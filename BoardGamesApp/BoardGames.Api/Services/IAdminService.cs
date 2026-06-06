// <copyright file="IAdminService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Shared.Common;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IAdminService
    {
        Task<ServiceResult<List<AccountProfileDTO>>> GetAllAccountsAsync(int page, int pageSize);

        Task<ServiceResult<bool>> SuspendAccountAsync(Guid accountId);

        Task<ServiceResult<bool>> UnsuspendAccountAsync(Guid accountId);

        Task<ServiceResult<bool>> ResetPasswordAsync(Guid accountId, string newPassword);

        Task<ServiceResult<bool>> UnlockAccountAsync(Guid accountId);
    }
}
