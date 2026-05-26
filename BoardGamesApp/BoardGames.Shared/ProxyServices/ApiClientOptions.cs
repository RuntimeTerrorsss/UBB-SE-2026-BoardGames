namespace BoardGames.Shared.ProxyServices
{
    public sealed class ApiClientOptions
    {
        public Uri? BaseAddress { get; set; }

        public TimeSpan? Timeout { get; set; }
    }
}
