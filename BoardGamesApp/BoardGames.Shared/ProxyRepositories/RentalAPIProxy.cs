// <copyright file="RentalRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;

namespace BoardGames.Shared.ProxyRepositories
{
    public class RentalAPIProxy : IRentalRepository
    {
        private readonly HttpClient httpClient;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public RentalAPIProxy(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Rental?> GetById(int rentalId)
        {
            var response = await httpClient.GetAsync($"rentals/{rentalId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Rental>(JsonOptions);
        }

        public async Task<TimeRange?> GetRentalTimeRange(int rentalId)
        {
            var response = await httpClient.GetAsync($"rentals/{rentalId}/timerange");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TimeRange>(JsonOptions);
        }

        public async Task<List<TimeRange>> GetAllOccupiedPeriods()
        {
            var response = await httpClient.GetAsync("rentals/occupied");
            if (!response.IsSuccessStatusCode)
            {
                return new List<TimeRange>();
            }

            return await response.Content.ReadFromJsonAsync<List<TimeRange>>(JsonOptions)
                   ?? new List<TimeRange>();
        }

        public async Task<List<TimeRange>> GetUnavailableTimeRanges(int gameId)
        {
            return await httpClient.GetFromJsonAsync<List<TimeRange>>(
                       $"rentals/game/{gameId}/unavailable", JsonOptions)
                   ?? new List<TimeRange>();
        }

        public async Task<bool> CheckGameAvailability(DateTime startTime, DateTime endTime, int gameId)
        {
            var range = new TimeRange(startTime, endTime);
            var response = await httpClient.PostAsJsonAsync($"rentals/{gameId}/check", range, JsonOptions);
            response.EnsureSuccessStatusCode();
            var available = await response.Content.ReadFromJsonAsync<bool>(JsonOptions);
            return available;
        }

        public async Task AddRental(Rental rental)
        {
            var response = await httpClient.PostAsJsonAsync("rentals", rental, JsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Rental>> GetRentalsForUser(int userId)
        {
            var response = await httpClient.GetAsync($"rentals/user/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                return new List<Rental>();
            }

            return await response.Content.ReadFromJsonAsync<List<Rental>>(JsonOptions)
                   ?? new List<Rental>();
        }

        public async Task BookGameWithRentalRequest(int clientId, int gameId, DateTime startDate, DateTime endDate)
        {
            var response = await httpClient.PostAsJsonAsync(
                "rentals/book",
                new { ClientId = clientId, GameId = gameId, StartDate = startDate, EndDate = endDate },
                JsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public ImmutableList<Rental> GetAll()
        {
            var rentals = httpClient.GetFromJsonAsync<List<Rental>>("rentals", JsonOptions).GetAwaiter().GetResult();
            return (rentals ?? new List<Rental>()).ToImmutableList();
        }

        public void Add(Rental rental)
        {
            var response = httpClient.PostAsJsonAsync("rentals", rental, JsonOptions).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public Rental Delete(int id)
        {
            var response = httpClient.DeleteAsync($"rentals/{id}").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<Rental>(JsonOptions).GetAwaiter().GetResult()
                   ?? new Rental { Id = id };
        }

        public void Update(int id, Rental updated)
        {
            var response = httpClient.PutAsJsonAsync($"rentals/{id}", updated, JsonOptions).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public Rental Get(int id)
        {
            return GetById(id).GetAwaiter().GetResult()
                   ?? throw new KeyNotFoundException($"Rental {id} was not found.");
        }

        public void AddConfirmed(Rental confirmedRental)
        {
            Add(confirmedRental);
        }

        public ImmutableList<Rental> GetRentalsByOwner(Guid ownerAccountId)
        {
            var rentals = httpClient.GetFromJsonAsync<List<Rental>>($"rentals/owner/{ownerAccountId}", JsonOptions)
                .GetAwaiter()
                .GetResult();
            return (rentals ?? new List<Rental>()).ToImmutableList();
        }

        public ImmutableList<Rental> GetRentalsByRenter(Guid renterAccountId)
        {
            var rentals = httpClient.GetFromJsonAsync<List<Rental>>($"rentals/renter/{renterAccountId}", JsonOptions)
                .GetAwaiter()
                .GetResult();
            return (rentals ?? new List<Rental>()).ToImmutableList();
        }

        public ImmutableList<Rental> GetRentalsByGame(int gameId)
        {
            var rentals = httpClient.GetFromJsonAsync<List<Rental>>($"rentals/games/{gameId}", JsonOptions)
                .GetAwaiter()
                .GetResult();
            return (rentals ?? new List<Rental>()).ToImmutableList();
        }
    }
}
