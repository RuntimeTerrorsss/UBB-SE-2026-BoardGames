// <copyright file="GamesRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BoardGames.Data;
using BoardGames.Data.Enums;
using BoardGames.Data.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
/// <summary>
/// Repository responsible for reading game/listing data from the database.
/// Important:
/// - This repository only reads data.
/// - It is used by the service layer, not directly by the UI.
/// How ADO.NET handles connections:
/// - When you write using var connection = new SqlConnection(...) and call .Open(), Microsoft checks the pool, so the pool of connections is handled by .net
/// - If there is a free connection, it gives it to you.
/// - When your "using" block finishes, it calls .Close().
/// - Microsoft intercepts your .Close() command. It doesn't actually destroy the connection to the database. It just wipes the data clean and parks it back in the hidden pool for.  the next person to use.
/// </summary>
public class GamesAPIProxy : InterfaceGamesRepository
{
    private readonly HttpClient httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public GamesAPIProxy(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<Game?> GetGameById(int gameId)
    {
        try
        {
            var response = await this.httpClient.GetAsync($"games/{gameId}");
            Debug.WriteLine($"GetGameById status: {response.StatusCode}");
            if (!response.IsSuccessStatusCode) return null;
            var raw = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"GetGameById raw: {raw}");
            return JsonSerializer.Deserialize<Game>(raw, JsonOptions);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetGameById EXCEPTION: {ex.Message}");
            Debug.WriteLine($"GetGameById INNER: {ex.InnerException?.Message}");
            return null;
        }
    }

    public async Task<decimal> GetPriceGameById(int gameId)
    {
        var response = await this.httpClient.GetAsync($"games/{gameId}/price");
        response.EnsureSuccessStatusCode();
        var raw = await response.Content.ReadAsStringAsync();
        return decimal.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
    }

    public async Task<List<Game>> GetAll()
    {
        return await this.httpClient.GetFromJsonAsync<List<Game>>("games", JsonOptions)
               ?? new List<Game>();
    }

    public async Task<List<Game>> GetGamesByFilter(FilterCriteria filter)
    {
        var response = await this.httpClient.PostAsJsonAsync("games/search", filter, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Game>>(JsonOptions)
               ?? new List<Game>();
    }

    public async Task<List<Game>> GetGamesForFeedAvailableTonight(int userId)
    {
        return await this.httpClient.GetFromJsonAsync<List<Game>>(
                   $"games/feed/tonight?userId={userId}", JsonOptions)
               ?? new List<Game>();
    }

    public async Task<List<Game>> GetRemainingGamesForFeed(int userId)
    {
        return await this.httpClient.GetFromJsonAsync<List<Game>>(
                   $"games/feed/remaining?userId={userId}", JsonOptions)
               ?? new List<Game>();
    }

    public void AddGame(Game game)
    {
        var response = this.httpClient.PostAsJsonAsync("games", game, JsonOptions).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
    }

    public Game DeleteGame(int id)
    {
        var response = this.httpClient.DeleteAsync($"games/{id}").GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        return response.Content.ReadFromJsonAsync<Game>(JsonOptions).GetAwaiter().GetResult()
               ?? new Game { Id = id };
    }

    public void UpdateGame(int id, Game updated)
    {
        var response = this.httpClient.PutAsJsonAsync($"games/{id}", updated, JsonOptions).GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
    }

    public Game GetGame(int id)
    {
        return GetGameById(id).GetAwaiter().GetResult()
               ?? throw new KeyNotFoundException($"Game {id} was not found.");
    }

    public ImmutableList<Game> GetGamesByOwner(Guid ownerAccountId)
    {
        var games = this.httpClient.GetFromJsonAsync<List<Game>>($"games/owner/{ownerAccountId}", JsonOptions)
            .GetAwaiter()
            .GetResult();
        return (games ?? new List<Game>()).ToImmutableList();
    }
}
