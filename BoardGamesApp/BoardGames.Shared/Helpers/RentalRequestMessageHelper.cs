// <copyright file="RentalRequestMessageHelper.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;

namespace BoardGames.Shared.Helpers
{
    public static partial class RentalRequestMessageHelper
    {
        public static int ResolveRequestId(int requestId, string content)
        {
            int fromContent = TryParseRequestIdFromContent(content);
            if (fromContent > 0)
            {
                return fromContent;
            }

            return requestId > 0 ? requestId : 0;
        }

        public static int TryParseRequestIdFromContent(string content)
        {
            var match = RequestIdPrefixRegex().Match(content ?? string.Empty);
            return match.Success && int.TryParse(match.Groups[1].Value, out int id) ? id : 0;
        }

        public static int ResolveRentalId(int rentalId, string content)
        {
            int fromContent = TryParseRentalIdFromContent(content);
            if (fromContent > 0)
            {
                return fromContent;
            }

            return rentalId > 0 ? rentalId : 0;
        }

        public static int TryParseRentalIdFromContent(string content)
        {
            var match = RentalIdTagRegex().Match(content ?? string.Empty);
            return match.Success && int.TryParse(match.Groups[1].Value, out int id) ? id : 0;
        }

        public static string EnsureRentalTag(string content, int rentalId)
        {
            if (rentalId <= 0 || TryParseRentalIdFromContent(content) > 0)
            {
                return content ?? string.Empty;
            }

            return $"[rental:{rentalId}]{content}";
        }

        [GeneratedRegex(@"^\[req:(\d+)\]")]
        private static partial Regex RequestIdPrefixRegex();

        [GeneratedRegex(@"\[rental:(\d+)\]")]
        private static partial Regex RentalIdTagRegex();
    }
}
