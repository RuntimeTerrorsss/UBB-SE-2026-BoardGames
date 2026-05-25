// <copyright file="RightPanelView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Desktop.ViewModels;

namespace BoardGames.Desktop.Views.ChatViews
{
    public sealed partial class RightPanelView : UserControl
    {
        public event EventHandler<(int UserId, int RequestId, int MessageId)>? ProceedToPaymentRequested;

        public RightPanelView()
        {
            this.InitializeComponent();

            this.ActiveChat.ProceedToPaymentRequested += (sender, paymentArguments) => this.ProceedToPaymentRequested?.Invoke(sender, paymentArguments);
        }

        public ChatViewModel ChatViewModel
        {
            set
            {
                this.ActiveChat.ViewModel = value;
            }
        }

        public int CurrentUserId
        {
            set => this.ActiveChat.CurrentUserId = value;
        }

        private bool isConversationSelected = false;

        public bool IsConversationSelected
        {
            get => this.isConversationSelected;
            set
            {
                this.isConversationSelected = value;
                this.WelcomePlaceholder.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                this.ActiveChat.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
