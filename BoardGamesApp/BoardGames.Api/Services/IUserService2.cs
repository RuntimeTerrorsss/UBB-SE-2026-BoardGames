// <copyright file="IUserService2.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IUserService
    {
        ImmutableList<UserDTO> GetUsersExcept(Guid excludeAccountId);
    }
}
