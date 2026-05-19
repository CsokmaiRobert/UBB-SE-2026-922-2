using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using BoardRentAndProperty.ViewModels;

namespace BoardRentAndProperty.Views
{
    public sealed partial class RentalsToOthersPage : Page
    {
        public RentalsToOthersPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (navigationEventArgs.Parameter is RentalsToOthersViewModel rentalsToOthersViewModel)
            {
                DataContext = rentalsToOthersViewModel;
                await rentalsToOthersViewModel.LoadRentalsAsync();
                return;
            }

            if (DataContext is not RentalsToOthersViewModel)
            {
                DataContext = App.Services.GetRequiredService<RentalsToOthersViewModel>();
            }

            if (DataContext is RentalsToOthersViewModel currentViewModel)
            {
                await currentViewModel.LoadRentalsAsync();
            }
        }

        private void CreateRentalButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            Frame?.Navigate(typeof(CreateRentalView));
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            ImageFailureHandler.HandleFailure(sender as Image, Resources);
        }

        private void NextButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            (DataContext as RentalsToOthersViewModel)?.NextPage();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            (DataContext as RentalsToOthersViewModel)?.PrevPage();
        }
    }
}
