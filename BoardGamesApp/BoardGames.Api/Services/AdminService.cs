// <copyright file="AdminService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Api.Security;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.Common;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAccountRepository accountRepository;
        private readonly IFailedLoginRepository failedLoginRepository;

        public AdminService(IAccountRepository accountRepository, IFailedLoginRepository failedLoginRepository)
        {
            this.accountRepository = accountRepository;
            this.failedLoginRepository = failedLoginRepository;
        }

        public async Task<ServiceResult<List<AccountProfileDTO>>> GetAllAccountsAsync(int pageNumber, int pageSize)
        {
            List<User> accountEntities = await this.accountRepository.GetAllAsync(pageNumber, pageSize);

            List<AccountProfileDTO> accountProfileDtos = new List<AccountProfileDTO>();

            foreach (User accountEntity in accountEntities)
            {
                Role? firstRole = accountEntity.Roles?.FirstOrDefault();
                FailedLoginAttempt? failedAttempt = await this.failedLoginRepository.GetByAccountIdAsync(accountEntity.Id);

                bool isLocked = failedAttempt != null
                    && failedAttempt.LockedUntil.HasValue
                    && failedAttempt.LockedUntil.Value > DateTime.UtcNow;

                accountProfileDtos.Add(new AccountProfileDTO
                {
                    Id = accountEntity.Id,
                    Username = accountEntity.Username,
                    DisplayName = accountEntity.DisplayName,
                    Email = accountEntity.Email,
                    PhoneNumber = accountEntity.PhoneNumber,
                    AvatarUrl = accountEntity.AvatarUrl,
                    Role = new RoleDTO
                    {
                        Id = firstRole?.Id ?? Guid.Empty,
                        Name = firstRole?.Name ?? "Standard User",
                    },
                    IsSuspended = accountEntity.IsSuspended,
                    IsLocked = isLocked,
                    Country = accountEntity.Country,
                    City = accountEntity.City,
                    StreetName = accountEntity.StreetName,
                    StreetNumber = accountEntity.StreetNumber,
                });
            }

            return ServiceResult<List<AccountProfileDTO>>.Ok(accountProfileDtos);
        }

        public async Task<ServiceResult<bool>> SuspendAccountAsync(Guid accountId)
        {
            User? accountEntity = await this.accountRepository.GetByIdAsync(accountId);
            if (accountEntity == null)
            {
                return ServiceResult<bool>.Fail("Account not found.");
            }

            accountEntity.IsSuspended = true;
            await this.accountRepository.UpdateAsync(accountEntity);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> UnsuspendAccountAsync(Guid accountId)
        {
            User? accountEntity = await this.accountRepository.GetByIdAsync(accountId);
            if (accountEntity == null)
            {
                return ServiceResult<bool>.Fail("Account not found.");
            }

            accountEntity.IsSuspended = false;
            await this.accountRepository.UpdateAsync(accountEntity);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> ResetPasswordAsync(Guid accountId, string newPassword)
        {
            const int MinimumPasswordLength = 6;
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < MinimumPasswordLength)
            {
                return ServiceResult<bool>.Fail("Password must be at least 6 characters long.");
            }

            User? accountEntity = await this.accountRepository.GetByIdAsync(accountId);
            if (accountEntity == null)
            {
                return ServiceResult<bool>.Fail("Account not found.");
            }

            accountEntity.PasswordHash = PasswordHasher.HashPassword(newPassword);
            await this.accountRepository.UpdateAsync(accountEntity);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> UnlockAccountAsync(Guid accountId)
        {
            await this.failedLoginRepository.ResetAsync(accountId);
            return ServiceResult<bool>.Ok(true);
        }
    }
}
