// <copyright file="RightPanelView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BookingBoardGames.Src.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace BookingBoardGames.Src.Views.ChatViews
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
