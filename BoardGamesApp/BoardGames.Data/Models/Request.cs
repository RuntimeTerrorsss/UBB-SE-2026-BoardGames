using System;
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
            Id = id;
            Game = requestedGame;
            Renter = renterAccount;
            Owner = ownerAccount;
            StartDate = startDate;
            EndDate = endDate;
            Status = status;
            OfferingUser = offeringUser;
        }
    }
}
