using Microsoft.AspNetCore.Http;

namespace BoardGames.Web.Infrastructure
{
    public interface IApiAuthCookieStore
    {
        void StoreFrom(HttpResponseMessage response);

        void ApplyTo(HttpRequestMessage request);

        void Clear();
    }

    public sealed class SessionApiAuthCookieStore : IApiAuthCookieStore
    {
        private const string SessionKey = "ApiAuthCookieHeader";

        private readonly IHttpContextAccessor httpContextAccessor;

        public SessionApiAuthCookieStore(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public void StoreFrom(HttpResponseMessage response)
        {
            ArgumentNullException.ThrowIfNull(response);

            if (!response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? setCookieHeaders))
            {
                return;
            }

            string cookieHeader = string.Join("; ", setCookieHeaders
                .Select(ExtractCookiePair)
                .Where(static value => !string.IsNullOrWhiteSpace(value)));

            if (string.IsNullOrWhiteSpace(cookieHeader))
            {
                return;
            }

            httpContextAccessor.HttpContext?.Session.SetString(SessionKey, cookieHeader);
        }

        public void ApplyTo(HttpRequestMessage request)
        {
            ArgumentNullException.ThrowIfNull(request);

            string? cookieHeader = httpContextAccessor.HttpContext?.Session.GetString(SessionKey);
            if (string.IsNullOrWhiteSpace(cookieHeader))
            {
                return;
            }

            request.Headers.Remove("Cookie");
            request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
        }

        public void Clear()
        {
            httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
        }

        private static string ExtractCookiePair(string setCookieHeader)
        {
            int separatorIndex = setCookieHeader.IndexOf(';');
            return separatorIndex >= 0
                ? setCookieHeader[..separatorIndex].Trim()
                : setCookieHeader.Trim();
        }
    }
}
