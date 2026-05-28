namespace BoardGames.Desktop.Views
{
    using BoardGames.Desktop.ViewModels;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    public sealed partial class SearchGamesPage : Page
    {
        public SearchGamesPage()
        {
            this.InitializeComponent();

            this.ViewModel = App.Services.GetRequiredService<SearchGamesViewModel>();
            this.DataContext = this.ViewModel;
            this.ViewModel.OnNavigateToLogin = () => App.NavigateTo(AppPage.Login);
        }

        public SearchGamesViewModel ViewModel { get; }

        protected override async void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);
            await this.ViewModel.LoadAsync();
        }
    }
}
