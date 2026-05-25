// <copyright file="ApiClientOptions.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.ProxyServices
{
    public sealed class ApiClientOptions
    {
        public Uri? BaseAddress { get; set; }

        public TimeSpan? Timeout { get; set; }
    }
}
