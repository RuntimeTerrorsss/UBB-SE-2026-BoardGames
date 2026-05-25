// <copyright file="ApiErrorEnvelope.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Web.Infrastructure
{
    public sealed class ApiErrorEnvelope
    {
        public string? Code { get; set; }

        public string? Error { get; set; }

        public int Status { get; set; }
    }
}
