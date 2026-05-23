// <copyright file="UserRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Data.Constants;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using BoardGames.Data;

namespace BoardGames.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext context;

        public UserRepository(AppDbContext appContext)
        {
            context = appContext;
        }

        public async Task<User?> GetById(int id)
        {
            return await context.Users.FirstOrDefaultAsync(user => user.Id == id);
        }

        public async Task<User?> GetGameById(int id)
        {
            return await GetById(id);
        }

        public async Task<List<User>> GetAll()
        {
            return await context.Users.ToListAsync();
        }

        public async Task SaveAddress(int id, Address address)
        {
            var foundUser = await context.Users.FirstOrDefaultAsync(user => user.Id == id);

            if (foundUser is null)
            {
                return;
            }

            foundUser.Country = address.Country;
            foundUser.City = address.City;
            foundUser.Street = address.Street;
            foundUser.StreetNumber = address.StreetNumber;

            await context.SaveChangesAsync();
        }

        public async Task<decimal> GetUserBalance(int userId)
        {
            return await context.Users
                .Where(user => user.Id == userId)
                .Select(user => (decimal?)user.Balance)
                .FirstOrDefaultAsync() ?? 0m;
        }

        public async Task UpdateBalance(int userId, decimal newBalance)
        {
            var foundUser = await context.Users.FirstOrDefaultAsync(user => user.Id == userId);

            if (foundUser is null)
            {
                return;
            }

            foundUser.Balance = newBalance;

            await context.SaveChangesAsync();
        }
        private async Task<User?> GetByIdentifier(string identifier)
        {
            return await context.Users
                .FirstOrDefaultAsync(user => user.Email == identifier || user.Username == identifier);
        }

        public async Task<User?> Login(string emailOrUsername, string password)
        {
            var user = await GetByIdentifier(emailOrUsername);

            if (user == null) return null;


            if (user.PasswordHash == password)
            {
                return user;
            }

            return null;
        }

        public async Task<bool> Register(User newUser)
        {
            var exists = await context.Users
                .AnyAsync(user => user.Username == newUser.Username || user.Email == newUser.Email);

            if (exists) return false;

            Console.WriteLine("--------------------------user can be created now");

            newUser.CreatedAt = DateTime.Now;
            await context.Users.AddAsync(newUser);

            var result = await context.SaveChangesAsync();
            return result > 0;
        }
    }
}