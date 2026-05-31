// <copyright file="ApiErrorResponse.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.Common
{
    public sealed class ApiErrorResponse
    {
        public string Code { get; init; } = "request_failed";

        public string Error { get; init; } = "Request failed.";

        public int Status { get; init; } = 400;
    }
}
