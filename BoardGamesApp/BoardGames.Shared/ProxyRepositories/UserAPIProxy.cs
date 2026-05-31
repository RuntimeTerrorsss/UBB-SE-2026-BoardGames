// <copyright file="UserAPIProxy.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using Microsoft.EntityFrameworkCore;

public class UserAPIProxy : IUserRepository
{
    private readonly HttpClient httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public UserAPIProxy(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<User?> GetById(int id)
    {
        var response = await this.httpClient.GetAsync($"users/{id}");
        Debug.WriteLine($"GetById status: {response.StatusCode}");
        if (!response.IsSuccessStatusCode) return null;
        var raw = await response.Content.ReadAsStringAsync();
        Debug.WriteLine($"GetById raw: {raw}");
        try
        {
            return JsonSerializer.Deserialize<User>(raw, JsonOptions);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetById DESERIALIZE ERROR: {ex.Message}");
            return null;
        }
    }

    public async Task<User?> GetGameById(int id)
    {
        return await this.GetById(id);
    }

    public async Task<List<User>> GetAll()
    {
        return await this.httpClient.GetFromJsonAsync<List<User>>("users", JsonOptions)
               ?? new List<User>();
    }

    public async Task SaveAddress(int id, Address address)
    {
        var response = await this.httpClient.PutAsJsonAsync($"users/{id}/address", address, JsonOptions);
        response.EnsureSuccessStatusCode();
    }

    public async Task<decimal> GetUserBalance(int userId)
    {
        var response = await this.httpClient.GetAsync($"users/{userId}/balance");
        response.EnsureSuccessStatusCode();
        var raw = await response.Content.ReadAsStringAsync();
        return decimal.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
    }

    public async Task UpdateBalance(int userId, decimal newBalance)
    {
        var response = await this.httpClient.PutAsJsonAsync($"users/{userId}/balance", newBalance, JsonOptions);
        response.EnsureSuccessStatusCode();
    }

    public async Task<User?> Login(string emailOrUsername, string password)
    {
        var loginData = new { emailOrUsername = emailOrUsername, password = password };
        var response = await this.httpClient.PostAsJsonAsync("users/login", loginData, JsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            return null; 
        }
        return await response.Content.ReadFromJsonAsync<User>(JsonOptions);
    }

    public async Task<bool> Register(User newUser)
    {
        Console.WriteLine("here");
        var response = await this.httpClient.PostAsJsonAsync("users/register", newUser, JsonOptions);
        Console.WriteLine("here2");
        return response.IsSuccessStatusCode;
    }
}
