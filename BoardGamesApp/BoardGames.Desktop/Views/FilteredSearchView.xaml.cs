
namespace BoardGames.Desktop.Views
{
    public sealed partial class FilteredSearchView : Page
    {
        public FilteredSearchView()
        {
            this.InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs navigationArgs)
        {
            base.OnNavigatedTo(navigationArgs);
            var criteria = navigationArgs.Parameter as FilterCriteria ?? new FilterCriteria();
            var viewModel = new FilteredSearchViewModel(App.SearchAndFilterService, App.GlobalGeographicalService);
            viewModel.OnGameSelectedRequest += gameId =>
            {
                this.Frame.Navigate(typeof(GameDetailsView), gameId);
            };
            viewModel.OnGoBackRequest += () => this.Frame.Navigate(typeof(DiscoveryView));
            await viewModel.Initialize(criteria);
            this.DataContext = viewModel;
        }
    }
}
