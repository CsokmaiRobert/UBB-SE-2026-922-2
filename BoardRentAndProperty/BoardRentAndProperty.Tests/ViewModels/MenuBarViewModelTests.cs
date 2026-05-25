using BoardRentAndProperty.Tests.Fakes;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class MenuBarViewModelTests
    {
        private FakeSessionContext sessionContext = null!;
        private MenuBarViewModel viewModel = null!;
        private AppPage? capturedNavigationTarget;
        private int navigationTriggerCount;

        [SetUp]
        public void SetUp()
        {
            this.sessionContext = new FakeSessionContext { Role = "Standard User" };
            this.viewModel = new MenuBarViewModel(new DesktopAuthorizationService(this.sessionContext));
            this.capturedNavigationTarget = null;
            this.navigationTriggerCount = 0;
        }

        [Test]
        public void Constructor_RegistersAllMainMenuEntriesForStandardUser()
        {
            var registeredMenuLabels = this.viewModel.NavigationActionsByMenuLabel.Keys;

            Assert.That(registeredMenuLabels, Does.Contain("My Games"));
            Assert.That(registeredMenuLabels, Does.Contain("Notifications"));
            Assert.That(registeredMenuLabels, Does.Contain("My Rentals"));
            Assert.That(registeredMenuLabels, Does.Contain("Others' Rentals"));
        }

        [Test]
        public void DependencyInjection_WithRegisteredDependencies_ResolvesMenuBarViewModel()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ISessionContext>(new FakeSessionContext { Role = "Standard User" });
            services.AddSingleton<IDesktopAuthorizationService, DesktopAuthorizationService>();
            services.AddSingleton<MenuBarViewModel>();

            using var provider = services.BuildServiceProvider();

            var resolvedViewModel = provider.GetRequiredService<MenuBarViewModel>();

            Assert.That(resolvedViewModel.NavigationActionsByMenuLabel.Keys, Does.Contain("My Games"));
        }

        [Test]
        public void SelectedPageName_RecognizedLabels_FireCorrespondingNavigationTargets()
        {
            this.viewModel.RequestNavigation += CaptureNavigationTarget;

            this.viewModel.SelectedPageName = "My Games";
            Assert.That(this.capturedNavigationTarget, Is.EqualTo(AppPage.Listings));

            this.viewModel.SelectedPageName = "Notifications";
            Assert.That(this.capturedNavigationTarget, Is.EqualTo(AppPage.Notifications));

            this.viewModel.SelectedPageName = "My Rentals";
            Assert.That(this.capturedNavigationTarget, Is.EqualTo(AppPage.RentalsFromOthers));

            this.viewModel.SelectedPageName = "Others' Rentals";
            Assert.That(this.capturedNavigationTarget, Is.EqualTo(AppPage.RentalsToOthers));
        }

        [Test]
        public void SelectedPageName_UnrecognisedOrRepeatedValues_DoNotFireExtraNavigations()
        {
            this.viewModel.RequestNavigation += IncrementNavigationTriggerCount;

            this.viewModel.SelectedPageName = "Unknown page";
            Assert.That(this.navigationTriggerCount, Is.EqualTo(0));

            this.viewModel.SelectedPageName = "My Rentals";
            this.viewModel.SelectedPageName = "My Rentals";
            Assert.That(this.navigationTriggerCount, Is.EqualTo(1));
        }

        private void CaptureNavigationTarget(AppPage selectedPage)
        {
            this.capturedNavigationTarget = selectedPage;
        }

        private void IncrementNavigationTriggerCount(AppPage selectedPage)
        {
            _ = selectedPage;
            this.navigationTriggerCount++;
        }
    }
}
