using BoardGames.Desktop.Services;
using BoardGames.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BoardGames.Desktop.Views
{
    public sealed partial class FilterLandingPage : Page
    {
        public FilterLandingPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            var sessionContext = App.Services.GetRequiredService<ISessionContext>();
            if (sessionContext.IsLoggedIn)
            {
                StatusText.Text = $"Signed in as {sessionContext.DisplayName} ({sessionContext.Role}).";
                LoginButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                StatusText.Text = "Browse anonymously, then sign in to unlock protected features.";
                LoginButton.Visibility = Visibility.Visible;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            App.NavigateTo(AppPage.Login);
        }
    }
}
