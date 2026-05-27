using System;

namespace BoardGames.Desktop.Services
{
    public sealed class CurrentUserContext : ICurrentUserContext
    {
        private readonly ISessionContext sessionContext;

        public CurrentUserContext(ISessionContext sessionContext)
        {
            this.sessionContext = sessionContext;
        }

        public Guid CurrentUserId => sessionContext.AccountId;

        public int? CurrentPamUserId => sessionContext.PamUserId;

        public bool IsLoggedIn => sessionContext.IsLoggedIn;
    }
}
