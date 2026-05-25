// <copyright file="RequestActionDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class RequestActionDTO
    {
        public Guid AccountId { get; set; }

        public string Reason { get; set; }
    }
}
