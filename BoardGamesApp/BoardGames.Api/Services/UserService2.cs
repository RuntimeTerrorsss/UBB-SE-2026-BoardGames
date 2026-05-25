using System;
using System.Collections.Immutable;
using System.Linq;
using BoardGames.Api.Mappers;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public class BoardRentUserService : IBoardRentUserService
    {
        private const int FirstPageNumber = 1;

        private readonly IAccountRepository accountRepository;
        private readonly UserMapper userMapper;

        public BoardRentUserService(IAccountRepository accountRepository, UserMapper userMapper)
        {
            this.accountRepository = accountRepository;
            this.userMapper = userMapper;
        }

        public ImmutableList<UserDTO> GetUsersExcept(Guid excludeAccountId)
        {
            var allAccounts = accountRepository.GetAllAsync(FirstPageNumber, int.MaxValue).GetAwaiter().GetResult();
            return allAccounts
                .Where(account => account.Id != excludeAccountId)
                .Select(account => userMapper.ToDTO(account)!)
                .ToImmutableList();
        }
    }
}
