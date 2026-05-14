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
            int pageSize = PagedViewModel<string>.PageSize;
            var viewModel = new FakePagedViewModel(BuildItems(pageSize * 3));

            Assert.That(viewModel.PageCount, Is.EqualTo(3));
        }

        [Test]
        public void PageCount_OneExtraItemBeyondFullPage_RoundsUp()
        {
            int pageSize = PagedViewModel<string>.PageSize;
            var viewModel = new FakePagedViewModel(BuildItems((pageSize * 3) + 1));

            Assert.That(viewModel.PageCount, Is.EqualTo(4));
        }

        [Test]
        public void NextPage_AlreadyOnLastPage_StaysOnLastPage()
        {
            int pageSize = PagedViewModel<string>.PageSize;
            var viewModel = new FakePagedViewModel(BuildItems(pageSize));

            viewModel.NextPage();

            Assert.That(viewModel.CurrentPage, Is.EqualTo(1));
        }

        [Test]
        public void PrevPage_AlreadyOnFirstPage_StaysOnFirstPage()
        {
            int pageSize = PagedViewModel<string>.PageSize;
            var viewModel = new FakePagedViewModel(BuildItems(pageSize * 3));

            viewModel.PrevPage();

            Assert.That(viewModel.CurrentPage, Is.EqualTo(1));
        }

        [Test]
        public void PrevPage_OnMiddlePage_GoesBackOne()
        {
            int pageSize = PagedViewModel<string>.PageSize;
            var viewModel = new FakePagedViewModel(BuildItems(pageSize * 3)) { CurrentPage = 2 };

            viewModel.PrevPage();

            Assert.That(viewModel.CurrentPage, Is.EqualTo(1));
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
