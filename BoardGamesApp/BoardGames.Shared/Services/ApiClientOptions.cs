using System;

namespace BoardGames.Shared.Services
{
    public sealed class ApiClientOptions
    {
        public Uri? BaseAddress { get; set; }

        public TimeSpan? Timeout { get; set; }
    }
}
