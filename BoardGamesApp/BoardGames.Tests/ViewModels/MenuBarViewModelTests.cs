using System;
using BoardGames.Tests.Fakes;
using BoardGames.Desktop.ViewModels;
using BoardGames.Desktop.Services;
using Moq;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class MenuBarViewModelTests
    {
        private Mock<IDesktopAuthorizationService> mockAuthService = null!;
        private MenuBarViewModel viewModel = null!;
        private AppPage? capturedNavigationTarget;

        [SetUp]
        public void SetUp()
        {
            this.mockAuthService = new Mock<IDesktopAuthorizationService>();
            this.mockAuthService.Setup(a => a.IsAdministrator).Returns(false);

            this.viewModel = new MenuBarViewModel(this.mockAuthService.Object);
            this.viewModel.RequestNavigation += (page) => this.capturedNavigationTarget = page;
            this.capturedNavigationTarget = null;
        }

        [Test]
        public void Constructor_WhenViewModelIsCreated_RegistersBaseMenuEntries()
        {
            var registeredMenuLabels = this.viewModel.NavigationActionsByMenuLabel.Keys;

            Assert.That(registeredMenuLabels, Does.Contain("Dashboard"));
            Assert.That(registeredMenuLabels, Does.Contain("My Games"));
            Assert.That(registeredMenuLabels, Does.Contain("Notifications"));
        }

        [Test]
        public void SelectedPageName_MyGames_FiresListingsNavigation()
        {
            this.viewModel.SelectedPageName = "My Games";
            Assert.That(this.capturedNavigationTarget, Is.EqualTo(AppPage.Listings));
        }

        [Test]
        public void SelectedPageName_Dashboard_FiresDashboardNavigation()
        {
            this.viewModel.SelectedPageName = "Dashboard";
            Assert.That(this.capturedNavigationTarget, Is.EqualTo(AppPage.Dashboard));
        }

        [Test]
        public void SelectedPageName_Admin_FiresAdminNavigation_WhenUserIsAdmin()
        {
            this.mockAuthService.Setup(a => a.IsAdministrator).Returns(true);
            this.viewModel = new MenuBarViewModel(this.mockAuthService.Object);
            this.viewModel.RequestNavigation += (page) => this.capturedNavigationTarget = page;

            this.viewModel.SelectedPageName = "Admin";

            Assert.That(this.capturedNavigationTarget, Is.EqualTo(AppPage.Admin));
        }

        [Test]
        public void SelectedPageName_UnrecognisedLabel_DoesNotFireNavigation()
        {
            this.viewModel.SelectedPageName = "Unknown page";
            Assert.That(this.capturedNavigationTarget, Is.Null);
        }
    }
}