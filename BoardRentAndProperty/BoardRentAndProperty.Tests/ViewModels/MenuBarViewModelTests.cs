using BoardRentAndProperty.Tests.Fakes;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class MenuBarViewModelTests
    {
        private FakeSessionContext sessionContext = null!;
        private MenuBarViewModel viewModel = null!;
        private AppPage? capturedNavigationTarget;
        private bool navigationWasTriggered;
        private int navigationTriggerCount;

        [SetUp]
        public void SetUp()
        {
            this.sessionContext = new FakeSessionContext { Role = "Standard User" };
            this.viewModel = new MenuBarViewModel(this.sessionContext);
            this.capturedNavigationTarget = null;
            this.navigationWasTriggered = false;
            this.navigationTriggerCount = 0;
        }

        [Test]
        public void Constructor_WhenViewModelIsCreated_RegistersMainMenuEntries()
        {
            var registeredMenuLabels = this.viewModel.NavigationActionsByMenuLabel.Keys;

            Assert.That(registeredMenuLabels, Does.Contain("My Games"));
            Assert.That(registeredMenuLabels, Does.Contain("Others' Requests"));
            Assert.That(registeredMenuLabels, Does.Contain("Others' Rentals"));
            Assert.That(registeredMenuLabels, Does.Contain("My Requests"));
            Assert.That(registeredMenuLabels, Does.Contain("My Rentals"));
            Assert.That(registeredMenuLabels, Does.Contain("Notifications"));
        }

        [Test]
        public void SelectedPageName_MyGames_FiresListingsNavigation()
        {
            this.viewModel.RequestNavigation += CaptureNavigationTarget;
            this.viewModel.SelectedPageName = "My Games";
            Assert.That(this.capturedNavigationTarget, Is.EqualTo(AppPage.Listings));
        }

        [Test]
        public void SelectedPageName_Notifications_FiresNotificationsNavigation()
        {
            this.viewModel.RequestNavigation += CaptureNavigationTarget;
            this.viewModel.SelectedPageName = "Notifications";
            Assert.That(this.capturedNavigationTarget, Is.EqualTo(AppPage.Notifications));
        }

        [Test]
        public void SelectedPageName_MyRentals_FiresRentalsFromOthersNavigation()
        {
            this.viewModel.RequestNavigation += CaptureNavigationTarget;
            this.viewModel.SelectedPageName = "My Rentals";
            Assert.That(this.capturedNavigationTarget, Is.EqualTo(AppPage.RentalsFromOthers));
        }

        [Test]
        public void SelectedPageName_OthersRentals_FiresRentalsToOthersNavigation()
        {
            this.viewModel.RequestNavigation += CaptureNavigationTarget;
            this.viewModel.SelectedPageName = "Others' Rentals";
            Assert.That(this.capturedNavigationTarget, Is.EqualTo(AppPage.RentalsToOthers));
        }

        [Test]
        public void SelectedPageName_UnrecognisedLabel_DoesNotFireNavigation()
        {
            this.viewModel.RequestNavigation += MarkNavigationAsTriggered;
            this.viewModel.SelectedPageName = "Unknown page";
            Assert.That(this.navigationWasTriggered, Is.False);
        }

        [Test]
        public void SelectedPageName_SetToSameValueTwice_FiresNavigationOnlyOnce()
        {
            this.viewModel.RequestNavigation += IncrementNavigationTriggerCount;
            this.viewModel.SelectedPageName = "My Rentals";
            this.viewModel.SelectedPageName = "My Rentals";
            Assert.That(this.navigationTriggerCount, Is.EqualTo(1));
        }

        private void CaptureNavigationTarget(AppPage selectedPage)
        {
            this.capturedNavigationTarget = selectedPage;
        }

        private void MarkNavigationAsTriggered(AppPage selectedPage)
        {
            _ = selectedPage;
            this.navigationWasTriggered = true;
        }

        private void IncrementNavigationTriggerCount(AppPage selectedPage)
        {
            _ = selectedPage;
            this.navigationTriggerCount++;
        }
    }
}
