// <copyright file="CreateRentalDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class CreateRentalDTO
    {
        public int GameId { get; set; }

        public Guid RenterAccountId { get; set; }

        public Guid OwnerAccountId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
