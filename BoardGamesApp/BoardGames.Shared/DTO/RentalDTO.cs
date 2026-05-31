// <copyright file="RentalDataTransferObject.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGames.Shared.DTO
{
    public class RentalDataTransferObject
    {
        public RentalDataTransferObject(
            int id,
            int gameId,
            string gameName,
            int clientId,
            string clientName,
            int ownerId,
            string ownerName,
            DateTime startDate,
            DateTime endDate,
            decimal price)
        {
            Id = id;
            GameId = gameId;
            GameName = gameName;
            ClientId = clientId;
            ClientName = clientName;
            OwnerId = ownerId;
            OwnerName = ownerName;
            StartDate = startDate;
            EndDate = endDate;
            Price = price;
        }

        public RentalDataTransferObject()
        {
            GameName = string.Empty;
            ClientName = string.Empty;
            OwnerName = string.Empty;
        }

        public int Id { get; set; }

        public int GameId { get; set; }

        public string GameName { get; set; }

        public int ClientId { get; set; }

        public string ClientName { get; set; }

        public int OwnerId { get; set; }

        public string OwnerName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }
    }
}
