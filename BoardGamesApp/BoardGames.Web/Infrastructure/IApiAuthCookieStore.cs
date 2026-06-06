// <copyright file="IApiAuthCookieStore.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Web.Infrastructure
{
    /// <summary>
    /// Persists API authentication cookies between the browser session and outbound API calls.
    /// The MVC app uses its own auth cookie; the API uses a separate cookie set on login.
    /// </summary>
    public interface IApiAuthCookieStore
    {
        bool HasStoredCookies();

        void ApplyToRequest(HttpRequestMessage request);

        void SaveFromResponse(HttpResponseMessage response, Uri requestUri);

        /// <summary>
        /// Aligns the API auth browser cookie lifetime with the MVC sign-in (remember me or session).
        /// </summary>
        void AlignBrowserCookieExpiration(bool rememberMe);

        void Clear();
    }
}
