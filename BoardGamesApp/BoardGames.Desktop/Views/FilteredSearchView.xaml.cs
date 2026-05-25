// <copyright file="FilteredSearchView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BookingBoardGames.Src.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FilteredSearchView : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilteredSearchView"/> class.
        /// </summary>
        public FilteredSearchView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
        /// </summary>
        /// <param name="navigationArgs">Event data that can be examined by overriding code.</param>
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
