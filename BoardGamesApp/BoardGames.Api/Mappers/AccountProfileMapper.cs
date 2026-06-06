// <copyright file="AccountProfileMapper.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Linq;
using BoardGames.Data.Models;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Mappers
{
    public class AccountProfileMapper
    {
        private const string StandardAccountRoleName = "Standard User";

        public AccountProfileDTO ToDataTransferObject(Account account)
        {
            if (account == null)
            {
                return null;
            }

            Role? primaryRole = account.Roles?.FirstOrDefault();

            return new AccountProfileDTO
            {
                Id = account.Id,
                PamUserId = account.PamUserId,
                Username = account.Username,
                DisplayName = account.DisplayName,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                AvatarUrl = account.AvatarUrl,
                Role = new RoleDTO
                {
                    Id = primaryRole?.Id ?? Guid.Empty,
                    Name = primaryRole?.Name ?? StandardAccountRoleName,
                },
                IsSuspended = account.IsSuspended,
                Country = account.Country,
                City = account.City,
                StreetName = account.StreetName,
                StreetNumber = account.StreetNumber,
            };
        }

        public void ApplyToEntity(Account account, AccountProfileDTO dataTransferObject)
        {
            account.DisplayName = dataTransferObject.DisplayName;
            account.Email = dataTransferObject.Email;
            account.PhoneNumber = dataTransferObject.PhoneNumber;
            account.Country = dataTransferObject.Country;
            account.City = dataTransferObject.City;
            account.StreetName = dataTransferObject.StreetName;
            account.StreetNumber = dataTransferObject.StreetNumber;

            if (!string.IsNullOrWhiteSpace(dataTransferObject.AvatarUrl))
            {
                account.AvatarUrl = dataTransferObject.AvatarUrl;
            }

            account.UpdatedAt = DateTime.UtcNow;
        }
    }
}
