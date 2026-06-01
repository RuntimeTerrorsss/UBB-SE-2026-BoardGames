// <copyright file="ShellViewModelTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Linq;
using BoardGames.Desktop.Services;
using BoardGames.Desktop.ViewModels;
using BoardGames.Tests.Fakes;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class ShellViewModelTests
    {
        [Test]
        public void Refresh_WhenAnonymous_ShowsSearchGamesLoginAndRegister()
        {
            var viewModel = BuildViewModel(new FakeSessionContext { IsLoggedIn = false });

            Assert.That(viewModel.NavigationItems.Select(item => item.Label), Is.EqualTo(new[]
            {
                "Homepage",
                "Login",
                "Register",
            }));
        }

        [Test]
        public void Refresh_WhenLoggedInStandardUser_ShowsProtectedRoutesWithoutAdmin()
        {
            var viewModel = BuildViewModel(new FakeSessionContext
            {
                AccountId = Guid.NewGuid(),
                IsLoggedIn = true,
                Role = AppRoles.StandardUser,
            });

            Assert.That(viewModel.NavigationItems.Select(item => item.Label), Is.EqualTo(new[]
            {
                "Homepage",
                "My Games",
                "Notifications",
                "Dashboard",
                "Chat",
                "Account",
                "Logout",
            }));
            Assert.That(viewModel.NavigationItems.Any(item => item.Route == AppPage.Admin), Is.False);
        }

        [Test]
        public void Refresh_WhenLoggedInAdministrator_IncludesAdminRoute()
        {
            var viewModel = BuildViewModel(new FakeSessionContext
            {
                AccountId = Guid.NewGuid(),
                IsLoggedIn = true,
                Role = AppRoles.Administrator,
            });

            Assert.That(viewModel.NavigationItems.Any(item => item.Route == AppPage.Admin), Is.True);
        }

        [Test]
        public void SetCurrentRoute_UpdatesCurrentRouteAndSelectedItem()
        {
            var viewModel = BuildViewModel(new FakeSessionContext { IsLoggedIn = false });

            viewModel.SetCurrentRoute(AppPage.Register);

            Assert.That(viewModel.CurrentRoute, Is.EqualTo(AppPage.Register));
            Assert.That(viewModel.SelectedItem?.Route, Is.EqualTo(AppPage.Register));
        }

        [Test]
        public void FindItem_WithKnownRoute_ReturnsMatchingNavigationItem()
        {
            var viewModel = BuildViewModel(new FakeSessionContext { IsLoggedIn = false });

            var navigationItem = viewModel.FindItem(AppPage.Login);

            Assert.That(navigationItem, Is.Not.Null);
            Assert.That(navigationItem!.Label, Is.EqualTo("Login"));
        }

        [Test]
        public void Refresh_WhenCurrentRouteStillExists_PreservesSelection()
        {
            var sessionContext = new FakeSessionContext
            {
                AccountId = Guid.NewGuid(),
                IsLoggedIn = true,
                Role = AppRoles.StandardUser,
            };
            var viewModel = BuildViewModel(sessionContext);
            viewModel.SetCurrentRoute(AppPage.Chat);

            viewModel.Refresh();

            Assert.That(viewModel.SelectedItem?.Route, Is.EqualTo(AppPage.Chat));
        }

        private static ShellViewModel BuildViewModel(FakeSessionContext sessionContext)
        {
            var authorizationService = new DesktopAuthorizationService(sessionContext);
            return new ShellViewModel(authorizationService);
        }
    }
}
