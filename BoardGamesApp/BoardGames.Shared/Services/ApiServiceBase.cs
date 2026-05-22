using System.Net.Http;

namespace BoardGames.Shared.Services
{
    public abstract class ApiServiceBase
    {
        private readonly IHttpClientFactory httpClientFactory;

        protected ApiServiceBase(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        protected HttpClient CreateClient() => httpClientFactory.CreateClient(ApiClientNames.BoardRentApi);
    }
}
