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

        private void Games_ItemClick(object sender, ItemClickEventArgs eventArgs)
        {
            if (eventArgs.ClickedItem is SearchGameCardViewModel selectedGame)
            {
                App.NavigateTo(AppPage.GameDetails, selectedGame.Id);
            }
        }
    }
}
