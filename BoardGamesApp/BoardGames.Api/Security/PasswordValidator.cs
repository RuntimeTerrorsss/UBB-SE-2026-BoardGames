// <copyright file="PasswordValidator.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;

namespace BoardGames.Api.Security
{
    public static class PasswordValidator
    {
        public static (bool IsValid, string? Error) Validate(string password)
        {
            const int MinimumPasswordLength = 8;

            if (string.IsNullOrWhiteSpace(password) || password.Length < MinimumPasswordLength)
            {
                return (false, "Password must be at least 8 characters long.");
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return (false, "Password must contain at least one uppercase letter.");
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return (false, "Password must contain at least one number.");
            }

            if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            {
                return (false, "Password must contain at least one special character.");
            }

            return (true, null);
        }
    }
}
