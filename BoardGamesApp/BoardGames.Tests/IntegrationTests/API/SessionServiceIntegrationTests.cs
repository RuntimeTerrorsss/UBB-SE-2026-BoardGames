// <copyright file="SessionServiceIntegrationTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using NUnit.Framework;

namespace BoardGames.Tests.IntegrationTests.Api
{
    [TestFixture]
    [Category("Integration")]
    public sealed class SessionServiceIntegrationTests
    {
        [Test]
        public void SessionService_InitialState_NotLoggedIn()
        {
            var session = new BoardGames.Api.Services.SessionService();

            Assert.That(session.IsLoggedIn, Is.False);
        }

        [Test]
        public void SessionService_SetUser_LogsInUser()
        {
            var session = new BoardGames.Api.Services.SessionService();

            session.SetUser(1, "user", "display");

            Assert.That(session.IsLoggedIn, Is.True);
            Assert.That(session.Username, Is.EqualTo("user"));
        }

        [Test]
        public void SessionService_Clear_ResetsUser()
        {
            var session = new BoardGames.Api.Services.SessionService();

            session.SetUser(1, "user", "display");
            session.Clear();

            Assert.That(session.IsLoggedIn, Is.False);
        }
    }
}
