// <copyright file="SessionApiAuthCookieStore.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Web.Infrastructure
{
    /// <summary>
    /// Stores API auth cookies in session and a companion browser cookie so they survive app restarts.
    /// </summary>
    public sealed class SessionApiAuthCookieStore : IApiAuthCookieStore
    {
        public const string BrowserCookieName = "BoardGames.ApiAuth";

        private const string SessionKey = "BoardRent.ApiAuthCookies";
        private const string RememberMeSessionKey = "BoardRent.RememberMe";
        private static readonly TimeSpan SessionCookieLifetime = TimeSpan.FromHours(8);
        private static readonly TimeSpan PersistentCookieLifetime = TimeSpan.FromDays(14);

        private readonly IHttpContextAccessor httpContextAccessor;

        public SessionApiAuthCookieStore(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public bool HasStoredCookies()
        {
            return !string.IsNullOrEmpty(this.GetStoredCookieHeader());
        }

        public void ApplyToRequest(HttpRequestMessage request)
        {
            string? cookieHeader = this.GetStoredCookieHeader();
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
            }
        }

        public void SaveFromResponse(HttpResponseMessage response, Uri requestUri)
        {
            HttpContext? context = this.httpContextAccessor.HttpContext;
            if (context is null)
            {
                return;
            }

            if (!response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? setCookieHeaders))
            {
                return;
            }

            Dictionary<string, string> jar = ParseCookieHeader(this.GetStoredCookieHeader());

            foreach (string setCookie in setCookieHeaders)
            {
                string nameValue = setCookie.Split(';', 2)[0].Trim();
                int equalsIndex = nameValue.IndexOf('=');
                if (equalsIndex > 0)
                {
                    string name = nameValue[..equalsIndex];
                    jar[name] = nameValue;
                }
            }

            if (jar.Count == 0)
            {
                this.Clear();
                return;
            }

            this.PersistCookieHeader(string.Join("; ", jar.Values));
        }

        public void AlignBrowserCookieExpiration(bool rememberMe)
        {
            HttpContext? context = this.httpContextAccessor.HttpContext;
            if (context is null)
            {
                return;
            }

            context.Session.SetString(RememberMeSessionKey, rememberMe ? "1" : "0");

            string? cookieHeader = this.GetStoredCookieHeader();
            if (string.IsNullOrEmpty(cookieHeader))
            {
                return;
            }

            this.WriteBrowserCookie(cookieHeader, rememberMe);
        }

        public void Clear()
        {
            HttpContext? context = this.httpContextAccessor.HttpContext;
            if (context is null)
            {
                return;
            }

            context.Session.Remove(SessionKey);
            context.Session.Remove(RememberMeSessionKey);
            context.Response.Cookies.Delete(BrowserCookieName);
        }

        private string? GetStoredCookieHeader()
        {
            HttpContext? context = this.httpContextAccessor.HttpContext;
            if (context is null)
            {
                return null;
            }

            string? fromSession = context.Session.GetString(SessionKey);
            if (!string.IsNullOrEmpty(fromSession))
            {
                return fromSession;
            }

            if (context.Request.Cookies.TryGetValue(BrowserCookieName, out string? fromBrowser)
                && !string.IsNullOrEmpty(fromBrowser))
            {
                context.Session.SetString(SessionKey, fromBrowser);
                return fromBrowser;
            }

            return null;
        }

        private void PersistCookieHeader(string cookieHeader)
        {
            HttpContext? context = this.httpContextAccessor.HttpContext;
            if (context is null)
            {
                return;
            }

            context.Session.SetString(SessionKey, cookieHeader);

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                this.WriteBrowserCookie(cookieHeader, this.IsRememberMe());
            }
        }

        private bool IsRememberMe()
        {
            return this.httpContextAccessor.HttpContext?.Session.GetString(RememberMeSessionKey) == "1";
        }

        private void WriteBrowserCookie(string cookieHeader, bool rememberMe)
        {
            HttpContext? context = this.httpContextAccessor.HttpContext;
            if (context is null)
            {
                return;
            }

            var options = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps,
                IsEssential = true,
                Expires = rememberMe
                    ? DateTimeOffset.UtcNow.Add(PersistentCookieLifetime)
                    : DateTimeOffset.UtcNow.Add(SessionCookieLifetime),
            };

            context.Response.Cookies.Append(BrowserCookieName, cookieHeader, options);
        }

        private static Dictionary<string, string> ParseCookieHeader(string? header)
        {
            var jar = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(header))
            {
                return jar;
            }

            foreach (string part in header.Split(';', StringSplitOptions.TrimEntries))
            {
                int equalsIndex = part.IndexOf('=');
                if (equalsIndex > 0)
                {
                    jar[part[..equalsIndex]] = part;
                }
            }

            return jar;
        }
    }
}
