// <copyright file="LeftPanelView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Desktop.ViewModels;

namespace BookingBoardGames.Src.Views.ChatViews
{
    public sealed partial class LeftPanelView : UserControl
    {
        private LeftPanelViewModel viewModel;

        public LeftPanelViewModel ViewModel
        {
            get => this.viewModel;
            set
            {
                this.viewModel = value;
                this.viewModel.PropertyChanged += (sender, propertyChangedEventArgs) => this.RefreshVisibility();
                this.RefreshVisibility();
            }
        }

        private void RefreshVisibility()
        {
            this.EmptyStatePanel.Visibility = this.ViewModel.IsEmptyStateVisible ? Visibility.Visible : Visibility.Collapsed;
            this.NoMatchesPanel.Visibility = this.ViewModel.IsNoMatchesVisible ? Visibility.Visible : Visibility.Collapsed;
            this.ConversationList.Visibility = this.ViewModel.IsListVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public LeftPanelView()
        {
            this.InitializeComponent();
        }

        private void ConversationList_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (this.ConversationList.SelectedItem is ConversationPreviewModel selectedConversation)
            {
                this.ViewModel.SelectedConversation = selectedConversation;
            }
        }
    }
}
