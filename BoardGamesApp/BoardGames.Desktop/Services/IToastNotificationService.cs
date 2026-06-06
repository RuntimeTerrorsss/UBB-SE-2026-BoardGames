// <copyright file="IToastNotificationService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.Services
{
    public interface IToastNotificationService
    {
        void Show(string notificationTitle, string notificationBody);
    }
}
