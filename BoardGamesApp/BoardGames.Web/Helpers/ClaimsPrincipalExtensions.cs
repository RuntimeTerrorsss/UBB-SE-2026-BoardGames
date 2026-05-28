// <copyright file="ClaimsPrincipalExtensions.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Security.Claims;

namespace BoardGames.Web.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetAccountId(this ClaimsPrincipal user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string? raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out Guid accountId))
            {
                throw new InvalidOperationException("Account id claim is missing or invalid.");
            }

            return accountId;
        }

        public static bool TryGetAccountId(this ClaimsPrincipal user, out Guid accountId)
        {
            accountId = Guid.Empty;
            string? raw = user?.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(raw) && Guid.TryParse(raw, out accountId);
        }

        public static int? GetPamUserId(this ClaimsPrincipal user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string? raw = user.FindFirstValue("PamUserId");
            if (!string.IsNullOrWhiteSpace(raw) && int.TryParse(raw, out int id))
            {
                return id;
            }

            return null;
        }

        public static bool TryGetPamUserId(this ClaimsPrincipal user, out int id)
        {
            id = -1;
            string? raw = user?.FindFirstValue("PamUserId");
            return !string.IsNullOrWhiteSpace(raw) && int.TryParse(raw, out id);
        }

        public static string GetDisplayName(this ClaimsPrincipal user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string? displayName = user.FindFirstValue("DisplayName");
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return displayName!;
            }

            string? userName = user.FindFirstValue(ClaimTypes.Name);
            return string.IsNullOrWhiteSpace(userName) ? string.Empty : userName!;
        }

        public static string? GetRoleName(this ClaimsPrincipal user)
        {
            return user?.FindFirstValue(ClaimTypes.Role);
        }

        public static bool IsAdministrator(this ClaimsPrincipal user)
        {
            return string.Equals(user.GetRoleName(), AppRoles.Administrator, StringComparison.Ordinal);
        }
    }
}
