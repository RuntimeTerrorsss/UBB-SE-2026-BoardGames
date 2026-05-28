using CommunityToolkit.Mvvm.ComponentModel;

namespace BoardGames.Desktop.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string errorMessage = string.Empty;
    }
}
