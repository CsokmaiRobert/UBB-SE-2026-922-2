using CommunityToolkit.Mvvm.ComponentModel;

namespace BoardRent.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage;
    }
}