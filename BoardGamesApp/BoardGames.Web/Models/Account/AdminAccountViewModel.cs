// <copyright file="AdminAccountViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace BoardGames.Web.Models.Account
{
    public class AdminAccountViewModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public RoleViewModel Role { get; set; } = new RoleViewModel();

        [JsonPropertyName("isSuspended")]
        public bool IsSuspended { get; set; }

        [JsonPropertyName("isLockedOut")]
        public bool IsLockedOut { get; set; }
    }
}
