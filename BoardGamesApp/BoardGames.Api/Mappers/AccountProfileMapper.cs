// <copyright file="AccountProfileMapper.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Mappers
{
    public class AccountProfileMapper
    {
        private const string StandardAccountRoleName = "Standard User";

        public AccountProfileDTO? ToDTO(User? account)
        {
            if (account == null)
            {
                return null;
            }

            Role? primaryRole = account.Roles?.FirstOrDefault();

            return new AccountProfileDTO
            {
                Id = account.Id,
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

        public void ApplyToEntity(User account, AccountProfileDTO DTO)
        {
            account.DisplayName = DTO.DisplayName;
            account.Email = DTO.Email;
            account.PhoneNumber = DTO.PhoneNumber;
            account.Country = DTO.Country;
            account.City = DTO.City;
            account.StreetName = DTO.StreetName;
            account.StreetNumber = DTO.StreetNumber;

            if (!string.IsNullOrWhiteSpace(DTO.AvatarUrl))
            {
                account.AvatarUrl = DTO.AvatarUrl;
            }

            account.UpdatedAt = DateTime.UtcNow;
        }
    }
}
