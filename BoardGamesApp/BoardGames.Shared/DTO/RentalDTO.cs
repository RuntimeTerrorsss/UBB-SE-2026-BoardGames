// <copyright file="RentalDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class RentalDTO
    {
        private const string ShortDateDisplayFormat = "dd/MM";
        private const string LongDateDisplayFormat = "dd/MM/yyyy";
        private const string StartDateLabelPrefix = "Start: ";
        private const string EndDateLabelPrefix = "End: ";

        public RentalDTO(
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
            this.Id = id;
            this.GameId = gameId;
            this.GameName = gameName;
            this.ClientId = clientId;
            this.ClientName = clientName;
            this.OwnerId = ownerId;
            this.OwnerName = ownerName;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Price = price;
            this.Game = new GameDTO { Id = gameId, Name = gameName };
            this.Renter = new UserDTO { PamUserId = clientId, DisplayName = clientName };
            this.Owner = new UserDTO { PamUserId = ownerId, DisplayName = ownerName };
        }

        public RentalDTO()
        {
            this.GameName = string.Empty;
            this.ClientName = string.Empty;
            this.OwnerName = string.Empty;
            this.Game = new GameDTO();
            this.Renter = new UserDTO();
            this.Owner = new UserDTO();
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

        public GameDTO Game { get; set; }

        public UserDTO Renter { get; set; }

        public UserDTO Owner { get; set; }

        public string StartDateDisplay => this.StartDate.ToString(ShortDateDisplayFormat);

        public string EndDateDisplay => this.EndDate.ToString(ShortDateDisplayFormat);

        public string StartDateDisplayLong => $"{StartDateLabelPrefix}{this.StartDate.ToString(LongDateDisplayFormat)}";

        public string EndDateDisplayLong => $"{EndDateLabelPrefix}{this.EndDate.ToString(LongDateDisplayFormat)}";

        public bool IsExpired => this.EndDate < DateTime.UtcNow;
    }
}
