using BoardGames.Desktop.ViewModels;

namespace BoardGames.Desktop.Views
{
    public sealed partial class RentalsFromOthersPage : Page
    {
        public RentalsFromOthersPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (navigationEventArgs.Parameter is RentalsFromOthersViewModel rentalsFromOthersViewModel)
            {
                DataContext = rentalsFromOthersViewModel;
                await rentalsFromOthersViewModel.LoadRentalsAsync();
                return;
            }

            if (DataContext is not RentalsFromOthersViewModel)
            {
                DataContext = App.Services.GetRequiredService<RentalsFromOthersViewModel>();
            }

            if (DataContext is RentalsFromOthersViewModel currentViewModel)
            {
                await currentViewModel.LoadRentalsAsync();
            }
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            ImageFailureHandler.HandleFailure(sender as Image, Resources);
        }

        private void NextButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            (DataContext as RentalsFromOthersViewModel)?.NextPage();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            (DataContext as RentalsFromOthersViewModel)?.PrevPage();
        }
    }
}
