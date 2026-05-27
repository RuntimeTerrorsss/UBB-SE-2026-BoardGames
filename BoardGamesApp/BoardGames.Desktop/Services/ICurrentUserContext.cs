using System;

namespace BoardGames.Desktop.Services
{
    public interface ICurrentUserContext
    {
        Guid CurrentUserId { get; }

        int? CurrentPamUserId { get; }

        bool IsLoggedIn { get; }
    }
}
