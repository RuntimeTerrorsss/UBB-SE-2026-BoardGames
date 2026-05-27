// <copyright file="RequestDTO.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Shared.DTO
{
    public class RequestDTO
    {
        private const string ShortDateDisplayFormat = "dd/MM";
        private const string LongDateDisplayFormat = "dd/MM/yyyy";
        private const string StartDateLabelPrefix = "Start: ";
        private const string EndDateLabelPrefix = "End: ";

        public int Id { get; set; }

        public GameDTO Game { get; set; }

        public UserDTO Renter { get; set; }

        public UserDTO Owner { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Open;

        public UserDTO OfferingUser { get; set; }

        public string StartDateDisplay => this.StartDate.ToString(ShortDateDisplayFormat);

        public string EndDateDisplay => this.EndDate.ToString(ShortDateDisplayFormat);

        public string StartDateDisplayLong => $"{StartDateLabelPrefix}{this.StartDate.ToString(LongDateDisplayFormat)}";

        public string EndDateDisplayLong => $"{EndDateLabelPrefix}{this.EndDate.ToString(LongDateDisplayFormat)}";

        public bool CanOffer => this.Status == RequestStatus.Open;

        public RequestDTO()
        {
        }
    }
}
