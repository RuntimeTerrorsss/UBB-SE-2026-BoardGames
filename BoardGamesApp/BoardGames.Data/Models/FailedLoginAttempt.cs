// <copyright file="FailedLoginAttempt.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Data.Models
{
    public class FailedLoginAttempt
    {
        public Guid AccountId { get; set; }

        public User Account { get; set; } = default!;

        public int FailedAttempts { get; set; }

        public DateTime? LockedUntil { get; set; }
    }
}
