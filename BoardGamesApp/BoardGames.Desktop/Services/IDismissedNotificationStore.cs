// <copyright file="IDismissedNotificationStore.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.Services
{
    public interface IDismissedNotificationStore
    {
        HashSet<int> Load(int currentUserId);

        void Save(int currentUserId, IEnumerable<int> dismissedNotificationIdentifiers);
    }
}
