using System.Collections.Immutable;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class PagedViewModelTests
    {
        [Test]
        public void PageCount_EmptyList_StillReturnsOne()
        {
            var viewModel = new FakePagedViewModel(BuildItems(0));

            Assert.That(viewModel.PageCount, Is.EqualTo(1));
        }

        [Test]
        public void PageCount_ItemsFillExactlyThreePages_ReturnsThree()
        {
            var viewModel = new FakePagedViewModel(BuildItems(9));

            Assert.That(viewModel.PageCount, Is.EqualTo(3));
        }

        [Test]
        public void PageCount_OneExtraItemBeyondFullPage_RoundsUp()
        {
            var viewModel = new FakePagedViewModel(BuildItems(10));

            Assert.That(viewModel.PageCount, Is.EqualTo(4));
        }

        [Test]
        public void NextPage_AlreadyOnLastPage_StaysOnLastPage()
        {
            var viewModel = new FakePagedViewModel(BuildItems(3));

            viewModel.NextPage();

            Assert.That(viewModel.CurrentPage, Is.EqualTo(1));
        }

        [Test]
        public void PrevPage_AlreadyOnFirstPage_StaysOnFirstPage()
        {
            var viewModel = new FakePagedViewModel(BuildItems(9));

            viewModel.PrevPage();

            Assert.That(viewModel.CurrentPage, Is.EqualTo(1));
        }

        [Test]
        public void PrevPage_OnMiddlePage_GoesBackOne()
        {
            var viewModel = new FakePagedViewModel(BuildItems(9)) { CurrentPage = 2 };

            viewModel.PrevPage();

            Assert.That(viewModel.CurrentPage, Is.EqualTo(1));
        }

        [Test]
        public void Reload_FirstPage_ExposesPageSizeItems()
        {
            var viewModel = new FakePagedViewModel(BuildItems(9)) { CurrentPage = 1 };

            viewModel.TriggerReload();

            Assert.That(viewModel.PagedItems, Has.Count.EqualTo(3));
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
