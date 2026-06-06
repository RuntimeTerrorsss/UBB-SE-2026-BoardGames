// <copyright file="DesktopAuthorizationServiceTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using BoardGames.Desktop.Services;
using BoardGames.Desktop.ViewModels;
using BoardGames.Desktop.Views;
using BoardGames.Tests.Fakes;
using NUnit.Framework;

namespace BoardGames.Tests.Services
{
    [TestFixture]
    public sealed class DesktopAuthorizationServiceTests
    {
        [Test]
        public void CanAccessPage_AnonymousUser_AllowsOnlyPublicPages()
        {
            var sessionContext = new FakeSessionContext
            {
                AccountId = Guid.Empty,
                IsLoggedIn = false,
                Role = AppRoles.StandardUser,
            };
            var systemUnderTest = new DesktopAuthorizationService(sessionContext);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(systemUnderTest.CanAccessPage(typeof(ShellPage)), Is.True);
                Assert.That(systemUnderTest.CanAccessPage(typeof(SearchGamesPage)), Is.True);
                Assert.That(systemUnderTest.CanAccessPage(typeof(GameDetailsPage)), Is.True);
                Assert.That(systemUnderTest.CanAccessPage(typeof(LoginPage)), Is.True);
                Assert.That(systemUnderTest.CanAccessPage(typeof(RegisterPage)), Is.True);
                Assert.That(systemUnderTest.CanAccessPage(typeof(PlaceholderPage)), Is.False);
            }
        }

        [Test]
        public void CanAccessPage_LoggedInUser_AllowsPlaceholderPage()
        {
            var sessionContext = new FakeSessionContext
            {
                AccountId = Guid.NewGuid(),
                IsLoggedIn = true,
                Role = AppRoles.StandardUser,
            };
            var systemUnderTest = new DesktopAuthorizationService(sessionContext);

            Assert.That(systemUnderTest.CanAccessPage(typeof(PlaceholderPage)), Is.True);
        }

        [Test]
        public void CanAccessRoute_AnonymousUser_AllowsOnlyPublicRoutes()
        {
            var sessionContext = new FakeSessionContext
            {
                AccountId = Guid.Empty,
                IsLoggedIn = false,
                Role = AppRoles.StandardUser,
            };
            var systemUnderTest = new DesktopAuthorizationService(sessionContext);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Filter), Is.True);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.GameDetails), Is.True);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Login), Is.True);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Register), Is.True);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Games), Is.False);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Notifications), Is.False);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Dashboard), Is.False);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Chat), Is.False);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Account), Is.False);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Admin), Is.False);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Logout), Is.False);
            }
        }

        [Test]
        public void CanAccessRoute_StandardUser_AllowsProtectedRoutesButDeniesAdmin()
        {
            var sessionContext = new FakeSessionContext
            {
                AccountId = Guid.NewGuid(),
                IsLoggedIn = true,
                Role = AppRoles.StandardUser,
            };
            var systemUnderTest = new DesktopAuthorizationService(sessionContext);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Games), Is.True);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Notifications), Is.True);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Dashboard), Is.True);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Chat), Is.True);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Account), Is.True);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Logout), Is.True);
                Assert.That(systemUnderTest.CanAccessRoute(AppPage.Admin), Is.False);
            }
        }

        [Test]
        public void CanAccessRoute_Administrator_AllowsAdminRoute()
        {
            var sessionContext = new FakeSessionContext
            {
                AccountId = Guid.NewGuid(),
                IsLoggedIn = true,
                Role = AppRoles.Administrator,
            };
            var systemUnderTest = new DesktopAuthorizationService(sessionContext);

            Assert.That(systemUnderTest.CanAccessRoute(AppPage.Admin), Is.True);
        }

        [Test]
        public void IsAdministrator_RoleIsAdministrator_ReturnsTrue()
        {
            var sessionContext = new FakeSessionContext
            {
                AccountId = Guid.NewGuid(),
                IsLoggedIn = true,
                Role = AppRoles.Administrator,
            };
            var systemUnderTest = new DesktopAuthorizationService(sessionContext);

            Assert.That(systemUnderTest.IsAdministrator, Is.True);
        }

        [Test]
        public void CurrentAccountId_SessionContainsAccountId_ReturnsAccountId()
        {
            var accountId = Guid.NewGuid();
            var sessionContext = new FakeSessionContext
            {
                AccountId = accountId,
                IsLoggedIn = true,
                Role = AppRoles.StandardUser,
            };
            var systemUnderTest = new DesktopAuthorizationService(sessionContext);

            Assert.That(systemUnderTest.CurrentAccountId, Is.EqualTo(accountId));
        }
    }
}
