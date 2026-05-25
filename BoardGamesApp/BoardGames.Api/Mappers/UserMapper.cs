// <copyright file="UserMapper.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Mappers
{
    public class UserMapper
    {
        public UserDTO? ToDTO(User? account)
        {
            if (account == null)
            {
                return null;
            }

            return new UserDTO
            {
                Id = account.Id,
                DisplayName = account.DisplayName,
            };
        }

        public User? ToModel(UserDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new User { Id = dto.Id, DisplayName = dto.DisplayName };
        }
    }
}
