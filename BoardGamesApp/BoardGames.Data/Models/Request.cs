// <copyright file="Request.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Enums;

namespace BoardGames.Data.Models
{
    public class Request
    {
        public int Id { get; set; }

        public Game? Game { get; set; }

        public User? Renter { get; set; }

        public User? Owner { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Open;

        public User? OfferingUser { get; set; }

        public Request()
        {
        }

        public Request(int id, Game? requestedGame, User? renterAccount, User? ownerAccount, DateTime startDate, DateTime endDate,
                       RequestStatus status = RequestStatus.Open, User? offeringUser = null)
        {
            this.Id = id;
            this.Game = requestedGame;
            this.Renter = renterAccount;
            this.Owner = ownerAccount;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Status = status;
            this.OfferingUser = offeringUser;
        }
    }
}
