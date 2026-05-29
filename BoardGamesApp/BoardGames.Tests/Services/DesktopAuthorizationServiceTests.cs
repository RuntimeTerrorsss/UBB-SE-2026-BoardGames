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
        public void CanAccessPage_WhenAnonymous_AllowsPublicPagesOnly()
        {
            var authorizationService = BuildService(new FakeSessionContext { IsLoggedIn = false });

            Assert.That(authorizationService.CanAccessPage(typeof(SearchGamesPage)), Is.True);
            Assert.That(authorizationService.CanAccessPage(typeof(LoginPage)), Is.True);
            Assert.That(authorizationService.CanAccessPage(typeof(RegisterPage)), Is.True);
            Assert.That(authorizationService.CanAccessPage(typeof(ShellPage)), Is.True);
            Assert.That(authorizationService.CanAccessPage(typeof(PlaceholderPage)), Is.False);
        }

        [Test]
        public void CanAccessRoute_WhenAnonymous_AllowsOnlyFilterLoginAndRegister()
        {
            var authorizationService = BuildService(new FakeSessionContext { IsLoggedIn = false });

            Assert.That(authorizationService.CanAccessRoute(AppPage.Filter), Is.True);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Login), Is.True);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Register), Is.True);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Games), Is.False);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Notifications), Is.False);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Dashboard), Is.False);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Chat), Is.False);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Account), Is.False);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Admin), Is.False);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Logout), Is.False);
        }

        [Test]
        public void CanAccessRoute_WhenLoggedInStandardUser_AllowsProtectedRoutesExceptAdmin()
        {
            var authorizationService = BuildService(new FakeSessionContext
            {
                AccountId = Guid.NewGuid(),
                IsLoggedIn = true,
                Role = AppRoles.StandardUser,
            });

            Assert.That(authorizationService.CanAccessRoute(AppPage.Games), Is.True);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Notifications), Is.True);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Dashboard), Is.True);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Chat), Is.True);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Account), Is.True);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Logout), Is.True);
            Assert.That(authorizationService.CanAccessRoute(AppPage.Admin), Is.False);
        }

        [Test]
        public void CanAccessRoute_WhenLoggedInAdministrator_AllowsAdminRoute()
        {
            var authorizationService = BuildService(new FakeSessionContext
            {
                AccountId = Guid.NewGuid(),
                IsLoggedIn = true,
                Role = AppRoles.Administrator,
            });

            Assert.That(authorizationService.CanAccessRoute(AppPage.Admin), Is.True);
            Assert.That(authorizationService.IsAdministrator, Is.True);
        }

        [Test]
        public void SessionDerivedProperties_ReflectCurrentSessionValues()
        {
            var accountId = Guid.NewGuid();
            var authorizationService = BuildService(new FakeSessionContext
            {
                AccountId = accountId,
                IsLoggedIn = true,
                Role = AppRoles.StandardUser,
            });

            Assert.That(authorizationService.CurrentAccountId, Is.EqualTo(accountId));
            Assert.That(authorizationService.IsLoggedIn, Is.True);
            Assert.That(authorizationService.IsAdministrator, Is.False);
        }

        private static DesktopAuthorizationService BuildService(FakeSessionContext sessionContext)
        {
            return new DesktopAuthorizationService(sessionContext);
        }
    }
}
