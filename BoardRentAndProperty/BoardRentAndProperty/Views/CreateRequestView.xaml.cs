using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.ViewModels;

namespace BoardRentAndProperty.Views
{
    public sealed partial class CreateRequestView : Page
    {
        public CreateRequestViewModel ViewModel { get; }

        public CreateRequestView()
        {
            ViewModel = App.Services.GetRequiredService<CreateRequestViewModel>();
            this.InitializeComponent();

            GamePicker.ItemsSource = ViewModel.AvailableGamesToRequest;
        }

        private void GamePicker_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            ViewModel.SelectedGame = GamePicker.SelectedItem as GameDTO;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            ViewModel.StartDate = StartDatePicker.Date;
            ViewModel.EndDate = EndDatePicker.Date;

            var submitResult = ViewModel.SubmitRequest();
            if (submitResult.IsSuccess)
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
                return;
            }

            await DialogHelper.ShowMessageAsync(
                this.XamlRoot,
                submitResult.DialogTitle,
                submitResult.DialogMessage);
        }
    }
}
