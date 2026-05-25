// <copyright file="DashboardView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BookingBoardGames.Src.Views.ChatViews;

namespace BookingBoardGames.Src.Views
{
    public sealed partial class DashboardView : Page
    {
        public DashboardView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationArgs)
        {
            base.OnNavigatedTo(navigationArgs);
            if (navigationArgs.Parameter is int userId)
            {
                SessionContext.GetInstance().UserId = userId;
            }
        }

        private void PaymentHistoryButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            this.Frame?.Navigate(typeof(PaymentHistoryView));
        }

        private void ChatButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            int currentUserId = SessionContext.GetInstance().UserId;
            var window1 = new Window();
            var frame1 = new Frame();
            window1.Content = frame1;
            window1.Title = "User " + currentUserId;
            frame1.Navigate(typeof(ChatPageView), currentUserId);
            window1.Activate();
        }

        private void SeeEmptyChat_Click(object sender, RoutedEventArgs routedArgs)
        {
            var window1 = new Window();
            var frame1 = new Frame();
            window1.Content = frame1;
            frame1.Navigate(typeof(ChatPageView), ((App)Application.Current).NoChatsUser);
            window1.Activate();
        }

        private void BackButton_Click(object sender, RoutedEventArgs routedArgs)
        {
            this.Frame.Navigate(typeof(DiscoveryView));
        }
    }
}
