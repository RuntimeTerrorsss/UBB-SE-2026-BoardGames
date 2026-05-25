// <copyright file="RentalAPIProxy.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Net.Http.Json;
using System.Text.Json;
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
            var response = await this.httpClient.GetAsync($"rentals/{rentalId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Rental>(JsonOptions);
        }

        public async Task<TimeRange?> GetRentalTimeRange(int rentalId)
        {
            var response = await this.httpClient.GetAsync($"rentals/{rentalId}/timerange");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TimeRange>(JsonOptions);
        }

        public async Task<List<TimeRange>> GetAllOccupiedPeriods()
        {
            var response = await this.httpClient.GetAsync("rentals/occupied");
            if (!response.IsSuccessStatusCode)
            {
                return new List<TimeRange>();
            }

            return await response.Content.ReadFromJsonAsync<List<TimeRange>>(JsonOptions)
                   ?? new List<TimeRange>();
        }

        public async Task<List<TimeRange>> GetUnavailableTimeRanges(int gameId)
        {
            return await this.httpClient.GetFromJsonAsync<List<TimeRange>>(
                       $"rentals/game/{gameId}/unavailable", JsonOptions)
                   ?? new List<TimeRange>();
        }

        public async Task<bool> CheckGameAvailability(DateTime startTime, DateTime endTime, int gameId)
        {
            var range = new TimeRange(startTime, endTime);
            var response = await this.httpClient.PostAsJsonAsync($"rentals/{gameId}/check", range, JsonOptions);
            response.EnsureSuccessStatusCode();
            var available = await response.Content.ReadFromJsonAsync<bool>(JsonOptions);
            return available;
        }

        public async Task AddRental(Rental rental)
        {
            var response = await this.httpClient.PostAsJsonAsync("rentals", rental, JsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Rental>> GetRentalsForUser(int userId)
        {
            var response = await this.httpClient.GetAsync($"rentals/user/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                return new List<Rental>();
            }

            return await response.Content.ReadFromJsonAsync<List<Rental>>(JsonOptions)
                   ?? new List<Rental>();
        }

        public async Task BookGameWithRentalRequest(int clientId, int gameId, DateTime startDate, DateTime endDate)
        {
            var response = await this.httpClient.PostAsJsonAsync(
                "rentals/book",
                new { ClientId = clientId, GameId = gameId, StartDate = startDate, EndDate = endDate },
                JsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public System.Collections.Immutable.ImmutableList<Rental> GetAll() => throw new NotSupportedException();

        public void Add(Rental rental) => throw new NotSupportedException();

        public Rental Delete(int id) => throw new NotSupportedException();

        public void Update(int id, Rental updated) => throw new NotSupportedException();

        public Rental Get(int id) => throw new NotSupportedException();

        public void AddConfirmed(Rental rental) => throw new NotSupportedException();

        public System.Collections.Immutable.ImmutableList<Rental> GetRentalsByOwner(Guid ownerAccountId) => throw new NotSupportedException();

        public System.Collections.Immutable.ImmutableList<Rental> GetRentalsByRenter(Guid renterAccountId) => throw new NotSupportedException();

        public System.Collections.Immutable.ImmutableList<Rental> GetRentalsByGame(int gameId) => throw new NotSupportedException();
    }
}
