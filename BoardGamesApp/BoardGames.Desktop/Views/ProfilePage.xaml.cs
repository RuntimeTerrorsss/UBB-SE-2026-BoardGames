namespace BoardGames.Desktop.Views
{
    using BoardGames.Desktop.ViewModels;

    public sealed partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            this.InitializeComponent();

            this.ViewModel = Ioc.Default.GetService<ProfileViewModel>();
            this.DataContext = this.ViewModel;

            this.ViewModel.OnSignOutSuccess = () =>
            {
                App.OnUserLoggedOut();
            };

            this.Loaded += async (object sender, RoutedEventArgs eventArguments) =>
            {
                await this.ViewModel.LoadProfileAsync();
            };
        }

        public ProfileViewModel ViewModel { get; }
    }
}