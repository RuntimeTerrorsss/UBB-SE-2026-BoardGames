// <copyright file="CurrentUserContext.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

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

        public Guid CurrentUserId => this.sessionContext.AccountId;

        public int? CurrentPamUserId => this.sessionContext.PamUserId;

        public bool IsLoggedIn => this.sessionContext.IsLoggedIn;
    }
}
