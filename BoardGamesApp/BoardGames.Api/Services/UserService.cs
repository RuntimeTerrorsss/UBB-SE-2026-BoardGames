// <copyright file="UserService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Immutable;
using System.Linq;
using BoardGames.Api.Mappers;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public class UserService : IUserService
    {
        private const int FirstPageNumber = 1;

        private readonly IAccountRepository accountRepository;
        private readonly UserMapper userMapper;

        public UserService(IAccountRepository accountRepository, UserMapper userMapper)
        {
            this.accountRepository = accountRepository;
            this.userMapper = userMapper;
        }

        public ImmutableList<UserDTO> GetUsersExcept(Guid excludeAccountId)
        {
            var allAccounts = this.accountRepository.GetAllAsync(FirstPageNumber, int.MaxValue).GetAwaiter().GetResult();
            return allAccounts
                .Where(account => account.Id != excludeAccountId)
                .Select(account => this.userMapper.ToDTO(account)!)
                .ToImmutableList();
        }
    }
}
