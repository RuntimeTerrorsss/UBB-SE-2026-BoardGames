// <copyright file="ICurrentUserContext.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

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
