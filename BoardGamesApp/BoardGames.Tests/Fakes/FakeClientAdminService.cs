// <copyright file="FakeClientAdminService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;

namespace BoardGames.Tests.Fakes
{
    internal sealed class FakeClientAdminService : IAdminService
    {
        public ServiceResult<IReadOnlyList<AccountProfileDTO>>? AccountsResult { get; set; }

        public ServiceResult? SuspendResult { get; set; } = ServiceResult.Ok();

        public ServiceResult? UnsuspendResult { get; set; } = ServiceResult.Ok();

        public ServiceResult? ResetPasswordResult { get; set; } = ServiceResult.Ok();

        public ServiceResult? UnlockResult { get; set; } = ServiceResult.Ok();

        public int GetAllAccountsCallCount { get; private set; }

        public int SuspendCallCount { get; private set; }

        public int UnsuspendCallCount { get; private set; }

        public int ResetPasswordCallCount { get; private set; }

        public int UnlockCallCount { get; private set; }

        public int LastPage { get; private set; }

        public int LastPageSize { get; private set; }

        public Guid? LastSuspendAccountId { get; private set; }

        public Guid? LastUnsuspendAccountId { get; private set; }

        public Guid? LastResetPasswordAccountId { get; private set; }

        public Guid? LastUnlockAccountId { get; private set; }

        public string? LastResetPasswordNewPassword { get; private set; }

        public Task<ServiceResult<IReadOnlyList<AccountProfileDTO>>> GetAllAccountsAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            this.GetAllAccountsCallCount++;
            this.LastPage = page;
            this.LastPageSize = pageSize;

            if (this.AccountsResult != null)
            {
                return Task.FromResult(this.AccountsResult);
            }

            return Task.FromResult(ServiceResult<IReadOnlyList<AccountProfileDTO>>.Ok(
                new List<AccountProfileDTO>() as IReadOnlyList<AccountProfileDTO>));
        }

        public Task<ServiceResult> SuspendAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            this.SuspendCallCount++;
            this.LastSuspendAccountId = accountId;

            if (this.SuspendResult != null)
            {
                return Task.FromResult(this.SuspendResult);
            }

            return Task.FromResult(ServiceResult.Ok());
        }

        public Task<ServiceResult> UnsuspendAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            this.UnsuspendCallCount++;
            this.LastUnsuspendAccountId = accountId;

            if (this.UnsuspendResult != null)
            {
                return Task.FromResult(this.UnsuspendResult);
            }

            return Task.FromResult(ServiceResult.Ok());
        }

        public Task<ServiceResult> ResetPasswordAsync(Guid accountId, string newPassword, CancellationToken cancellationToken = default)
        {
            this.ResetPasswordCallCount++;
            this.LastResetPasswordAccountId = accountId;
            this.LastResetPasswordNewPassword = newPassword;

            if (this.ResetPasswordResult != null)
            {
                return Task.FromResult(this.ResetPasswordResult);
            }

            return Task.FromResult(ServiceResult.Ok());
        }

        public Task<ServiceResult> UnlockAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            this.UnlockCallCount++;
            this.LastUnlockAccountId = accountId;

            if (this.UnlockResult != null)
            {
                return Task.FromResult(this.UnlockResult);
            }

            return Task.FromResult(ServiceResult.Ok());
        }
    }
}