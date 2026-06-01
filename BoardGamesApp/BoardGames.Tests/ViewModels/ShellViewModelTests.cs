// <copyright file="ShellViewModelTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Linq;
using BoardGames.Desktop.Services;
using BoardGames.Desktop.ViewModels;
using Moq;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class ShellViewModelTests
    {
        [Test]
        public void Constructor_AnonymousUser_ShowsFilterLoginAndRegister()
        {
            var authorizationService = CreateAuthorizationService(isLoggedIn: false, isAdministrator: false);

            var systemUnderTest = new ShellViewModel(authorizationService.Object);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(systemUnderTest.NavigationItems.Select(item => item.Route), Is.EqualTo(new[]
                {
                    AppPage.Filter,
                    AppPage.Login,
                    AppPage.Register,
                }));
                Assert.That(systemUnderTest.SelectedItem?.Route, Is.EqualTo(AppPage.Filter));
                Assert.That(systemUnderTest.CurrentRoute, Is.EqualTo(AppPage.Filter));
            }
        }

        [Test]
        public void Refresh_LoggedInStandardUser_ShowsProtectedMenuWithoutAdmin()
        {
            var authorizationService = CreateAuthorizationService(isLoggedIn: true, isAdministrator: false);
            var systemUnderTest = new ShellViewModel(authorizationService.Object);

            systemUnderTest.Refresh();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(systemUnderTest.NavigationItems.Select(item => item.Route), Is.EqualTo(new[]
                {
                    AppPage.Filter,
                    AppPage.Games,
                    AppPage.Notifications,
                    AppPage.Dashboard,
                    AppPage.Chat,
                    AppPage.Account,
                    AppPage.Logout,
                }));
                Assert.That(systemUnderTest.FindItem(AppPage.Admin), Is.Null);
            }
        }

        [Test]
        public void Refresh_Administrator_AddsAdminNavigationItem()
        {
            var authorizationService = CreateAuthorizationService(isLoggedIn: true, isAdministrator: true);
            var systemUnderTest = new ShellViewModel(authorizationService.Object);

            systemUnderTest.Refresh();

            Assert.That(systemUnderTest.FindItem(AppPage.Admin), Is.Not.Null);
        }

        [Test]
        public void SetCurrentRoute_VisibleRoute_SelectsMatchingNavigationItem()
        {
            var authorizationService = CreateAuthorizationService(isLoggedIn: true, isAdministrator: false);
            var systemUnderTest = new ShellViewModel(authorizationService.Object);

            systemUnderTest.SetCurrentRoute(AppPage.Dashboard);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(systemUnderTest.CurrentRoute, Is.EqualTo(AppPage.Dashboard));
                Assert.That(systemUnderTest.SelectedItem?.Route, Is.EqualTo(AppPage.Dashboard));
            }
        }

        [Test]
        public void SetCurrentRoute_HiddenRoute_ClearsSelection()
        {
            var authorizationService = CreateAuthorizationService(isLoggedIn: false, isAdministrator: false);
            var systemUnderTest = new ShellViewModel(authorizationService.Object);

            systemUnderTest.SetCurrentRoute(AppPage.Dashboard);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(systemUnderTest.CurrentRoute, Is.EqualTo(AppPage.Dashboard));
                Assert.That(systemUnderTest.SelectedItem, Is.Null);
            }
        }

        [Test]
        public void FindItem_MissingRoute_ReturnsNull()
        {
            var authorizationService = CreateAuthorizationService(isLoggedIn: false, isAdministrator: false);
            var systemUnderTest = new ShellViewModel(authorizationService.Object);

            var result = systemUnderTest.FindItem(AppPage.Admin);

            Assert.That(result, Is.Null);
        }

        private static Mock<IDesktopAuthorizationService> CreateAuthorizationService(bool isLoggedIn, bool isAdministrator)
        {
            var authorizationService = new Mock<IDesktopAuthorizationService>();
            authorizationService.SetupGet(service => service.IsLoggedIn).Returns(isLoggedIn);
            authorizationService.SetupGet(service => service.IsAdministrator).Returns(isAdministrator);
            return authorizationService;
        }
    }
}
