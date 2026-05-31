// <copyright file="IDesktopAuthorizationService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using BoardGames.Desktop.ViewModels;

namespace BoardGames.Desktop.Services
{
    public interface IDesktopAuthorizationService
    {
        Guid CurrentAccountId { get; }

        bool IsLoggedIn { get; }

        bool IsAdministrator { get; }

        bool CanAccessPage(Type pageType);

        bool CanAccessRoute(AppPage page);
    }
}
