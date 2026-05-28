// <copyright file="FakeCurrentUserContext.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using BoardGames.Desktop.Services;

namespace BoardGames.Tests.Fakes
{
    internal sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public Guid CurrentUserId { get; set; }

        public int? CurrentPamUserId { get; set; }

        public bool IsLoggedIn { get; set; }
    }
}
