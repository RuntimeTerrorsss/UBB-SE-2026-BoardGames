// <copyright file="CurrentUserContextTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using BoardGames.Desktop.Services;
using BoardGames.Tests.Fakes;
using NUnit.Framework;

namespace BoardGames.Tests.Services
{
    [TestFixture]
    public sealed class CurrentUserContextTests
    {
        [Test]
        public void CurrentUserId_SessionContainsAccountId_ReturnsAccountId()
        {
            var accountId = Guid.NewGuid();
            var sessionContext = new FakeSessionContext { AccountId = accountId };
            var systemUnderTest = new CurrentUserContext(sessionContext);

            Assert.That(systemUnderTest.CurrentUserId, Is.EqualTo(accountId));
        }

        [Test]
        public void CurrentPamUserId_SessionContainsPamUserId_ReturnsPamUserId()
        {
            var sessionContext = new FakeSessionContext { PamUserId = 21 };
            var systemUnderTest = new CurrentUserContext(sessionContext);

            Assert.That(systemUnderTest.CurrentPamUserId, Is.EqualTo(21));
        }

        [Test]
        public void IsLoggedIn_SessionIsAnonymous_ReturnsFalse()
        {
            var sessionContext = new FakeSessionContext { IsLoggedIn = false };
            var systemUnderTest = new CurrentUserContext(sessionContext);

            Assert.That(systemUnderTest.IsLoggedIn, Is.False);
        }

        [Test]
        public void IsLoggedIn_SessionIsPopulated_ReturnsTrue()
        {
            var sessionContext = new FakeSessionContext { IsLoggedIn = true };
            var systemUnderTest = new CurrentUserContext(sessionContext);

            Assert.That(systemUnderTest.IsLoggedIn, Is.True);
        }
    }
}
