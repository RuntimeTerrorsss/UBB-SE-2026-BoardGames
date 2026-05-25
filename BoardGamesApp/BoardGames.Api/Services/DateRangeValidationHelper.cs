// <copyright file="DateRangeValidationHelper.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Api.Services
{
    internal static class DateRangeValidationHelper
    {
        public static bool HasValidFutureDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate.Date >= endDate.Date)
            {
                return false;
            }

            return startDate.Date >= DateTime.UtcNow.Date;
        }
    }
}
