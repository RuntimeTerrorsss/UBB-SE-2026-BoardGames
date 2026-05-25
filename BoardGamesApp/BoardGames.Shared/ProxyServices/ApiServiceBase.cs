namespace BoardGames.Shared.ProxyServices
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
