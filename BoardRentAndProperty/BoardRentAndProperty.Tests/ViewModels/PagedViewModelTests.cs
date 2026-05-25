using System.Collections.Immutable;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class PagedViewModelTests
    {
        [Test]
        public void PageCount_HandlesEmptyAndFilledAndPartialPages()
        {
            int pageSize = PagedViewModel<string>.PageSize;
            var emptyViewModel = new FakePagedViewModel(BuildItems(0));
            Assert.That(emptyViewModel.PageCount, Is.EqualTo(1));

            var fullPagesViewModel = new FakePagedViewModel(BuildItems(pageSize * 3));
            Assert.That(fullPagesViewModel.PageCount, Is.EqualTo(3));

            var oneExtraItemViewModel = new FakePagedViewModel(BuildItems((pageSize * 3) + 1));
            Assert.That(oneExtraItemViewModel.PageCount, Is.EqualTo(4));
        }

        [Test]
        public void NextPage_OnLastPage_StaysOnLastPage()
        {
            int pageSize = PagedViewModel<string>.PageSize;
            var viewModel = new FakePagedViewModel(BuildItems(pageSize));

            viewModel.NextPage();

            Assert.That(viewModel.CurrentPage, Is.EqualTo(1));
        }

        [Test]
        public void PrevPage_OnFirstPageStaysAndOnMiddlePageGoesBackOne()
        {
            int pageSize = PagedViewModel<string>.PageSize;
            var firstPageViewModel = new FakePagedViewModel(BuildItems(pageSize * 3));
            firstPageViewModel.PrevPage();
            Assert.That(firstPageViewModel.CurrentPage, Is.EqualTo(1));

            var middlePageViewModel = new FakePagedViewModel(BuildItems(pageSize * 3)) { CurrentPage = 2 };
            middlePageViewModel.PrevPage();
            Assert.That(middlePageViewModel.CurrentPage, Is.EqualTo(1));
        }

        [Test]
        public void Reload_FirstPage_ExposesPageSizeItems()
        {
            int pageSize = PagedViewModel<string>.PageSize;
            var viewModel = new FakePagedViewModel(BuildItems(pageSize * 3)) { CurrentPage = 1 };

            viewModel.TriggerReload();

            Assert.That(viewModel.PagedItems, Has.Count.EqualTo(pageSize));
        }

        private static ImmutableList<string> BuildItems(int count)
        {
            var builder = ImmutableList.CreateBuilder<string>();
            for (int itemIndex = 0; itemIndex < count; itemIndex++)
            {
                builder.Add($"item-{itemIndex}");
            }

            return builder.ToImmutable();
        }

        private sealed class FakePagedViewModel : PagedViewModel<string>
        {
            private readonly ImmutableList<string> items;

            public FakePagedViewModel(ImmutableList<string> items)
            {
                this.items = items;
                Reload();
            }

            public void TriggerReload()
            {
                Reload();
            }

            protected override void Reload()
            {
                SetAllItems(this.items);
            }
        }
    }
}
