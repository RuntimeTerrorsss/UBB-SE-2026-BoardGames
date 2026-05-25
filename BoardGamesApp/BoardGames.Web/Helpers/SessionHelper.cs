// <copyright file="SessionHelper.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Web.Helpers
{
    public class SessionHelper
    {
        private const string UserIdKey = "UserId";
        private const string UsernameKey = "Username";
        private const string DisplayNameKey = "DisplayName";

        public static void SetUser(ISession session, int id, string username, string displayName)
        {
            session.SetInt32(UserIdKey, id);
            session.SetString(UsernameKey, username);
            session.SetString(DisplayNameKey, displayName);
        }

        public static void Clear(ISession session) => session.Clear();

        public static int? GetUserId(ISession session) => session.GetInt32(UserIdKey);

        public static string? GetUsername(ISession session) => session.GetString(UsernameKey);

        public static string? GetDisplayName(ISession session) => session.GetString(DisplayNameKey);

        public static bool IsLoggedIn(ISession session) => session.GetInt32(UserIdKey).HasValue;
    }
}
