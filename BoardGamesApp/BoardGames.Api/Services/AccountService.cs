// <copyright file="AccountService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Api.Mappers;
using BoardGames.Api.Security;
using BoardGames.Data.Repositories;
using BoardGames.Shared.Common;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public class AccountService : IAccountService
    {
        private const int MinimumDisplayNameLength = 2;
        private const int MaximumDisplayNameLength = 50;
        private const int MaximumStreetNumberLength = 10;

        private readonly IAccountRepository accountRepository;
        private readonly AccountProfileMapper accountProfileMapper;
        private readonly IAvatarStorageService avatarStorageService;
        private readonly IFailedLoginRepository failedLoginRepository;

        public AccountService(
            IAccountRepository accountRepository,
            AccountProfileMapper accountProfileMapper,
            IAvatarStorageService avatarStorageService,
            IFailedLoginRepository failedLoginRepository)
        {
            this.accountRepository = accountRepository;
            this.accountProfileMapper = accountProfileMapper;
            this.avatarStorageService = avatarStorageService;
            this.failedLoginRepository = failedLoginRepository;
        }

        public async Task<ServiceResult<AccountProfileDTO>> GetProfileAsync(Guid accountId)
        {
            var accountEntity = await this.accountRepository.GetByIdAsync(accountId);
            if (accountEntity == null)
            {
                return ServiceResult<AccountProfileDTO>.Fail("Account not found.");
            }

            var failedAttempt = await this.failedLoginRepository.GetByAccountIdAsync(accountId);

            bool isLocked = failedAttempt?.LockedUntil.HasValue == true
                && failedAttempt.LockedUntil.Value > DateTime.UtcNow;

            var dto = this.accountProfileMapper.ToDataTransferObject(accountEntity)!;
            dto.IsLocked = isLocked;

            return ServiceResult<AccountProfileDTO>.Ok(dto);
        }

        public async Task<ServiceResult<bool>> UpdateProfileAsync(Guid accountId, AccountProfileDTO profileUpdateData)
        {
            var accountEntity = await this.accountRepository.GetByIdAsync(accountId);
            if (accountEntity == null)
            {
                return ServiceResult<bool>.Fail("Account not found.");
            }

            var validationErrors = ValidateProfileDetails(profileUpdateData);

            if (!string.IsNullOrWhiteSpace(profileUpdateData.Email) && profileUpdateData.Email != accountEntity.Email)
            {
                var accountWithDuplicateEmail = await this.accountRepository.GetByEmailAsync(profileUpdateData.Email);
                if (accountWithDuplicateEmail != null && accountWithDuplicateEmail.Id != accountId)
                {
                    validationErrors.Add("Email|This email address is already taken by another account.");
                }
            }

            if (validationErrors.Any())
            {
                return ServiceResult<bool>.Fail(string.Join(";", validationErrors));
            }

            this.accountProfileMapper.ApplyToEntity(accountEntity, profileUpdateData);
            await this.accountRepository.UpdateAsync(accountEntity);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword)
        {
            var accountEntity = await this.accountRepository.GetByIdAsync(accountId);
            if (accountEntity == null)
            {
                return ServiceResult<bool>.Fail("Account not found.");
            }

            if (!PasswordHasher.VerifyPassword(currentPassword, accountEntity.PasswordHash))
            {
                return ServiceResult<bool>.Fail("Current password is incorrect.");
            }

            var (isPasswordValid, passwordErrorMessage) = PasswordValidator.Validate(newPassword);
            if (!isPasswordValid)
            {
                return ServiceResult<bool>.Fail(passwordErrorMessage ?? "Password is invalid.");
            }

            accountEntity.PasswordHash = PasswordHasher.HashPassword(newPassword);
            accountEntity.UpdatedAt = DateTime.UtcNow;

            await this.accountRepository.UpdateAsync(accountEntity);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<string>> SetAvatarUrlAsync(Guid accountId, string avatarRelativeUrl)
        {
            var accountEntity = await this.accountRepository.GetByIdAsync(accountId);
            if (accountEntity == null)
            {
                return ServiceResult<string>.Fail("Account not found.");
            }

            accountEntity.AvatarUrl = avatarRelativeUrl;
            accountEntity.UpdatedAt = DateTime.UtcNow;

            await this.accountRepository.UpdateAsync(accountEntity);

            return ServiceResult<string>.Ok(avatarRelativeUrl);
        }

        public async Task<ServiceResult<bool>> RemoveAvatarAsync(Guid accountId)
        {
            var accountEntity = await this.accountRepository.GetByIdAsync(accountId);
            if (accountEntity == null)
            {
                return ServiceResult<bool>.Fail("Account not found.");
            }

            if (!string.IsNullOrWhiteSpace(accountEntity.AvatarUrl))
            {
                this.avatarStorageService.Delete(accountEntity.AvatarUrl);
            }

            accountEntity.AvatarUrl = string.Empty;
            accountEntity.UpdatedAt = DateTime.UtcNow;

            await this.accountRepository.UpdateAsync(accountEntity);

            return ServiceResult<bool>.Ok(true);
        }

        private static List<string> ValidateProfileDetails(AccountProfileDTO profileData)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(profileData.DisplayName) ||
                profileData.DisplayName.Length < MinimumDisplayNameLength ||
                profileData.DisplayName.Length > MaximumDisplayNameLength)
            {
                errors.Add("DisplayName|Display name must be between 2 and 50 characters long.");
            }

            if (!string.IsNullOrWhiteSpace(profileData.PhoneNumber))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(profileData.PhoneNumber, @"^\+?\d{7,15}$"))
                {
                    errors.Add("PhoneNumber|Phone number format is invalid.");
                }
            }

            if (!string.IsNullOrWhiteSpace(profileData.StreetNumber) && profileData.StreetNumber.Length > MaximumStreetNumberLength)
            {
                errors.Add("StreetNumber|Street number must be a valid value.");
            }

            return errors;
        }
    }
}
