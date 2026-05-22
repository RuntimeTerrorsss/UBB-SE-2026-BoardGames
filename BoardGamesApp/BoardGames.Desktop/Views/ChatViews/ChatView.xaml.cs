// <copyright file="ChatView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BoardGames.Desktop.ViewModels;
using BookingBoardGames.Data.Enum;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Src.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace BookingBoardGames.Src.Views.ChatViews
{
    public sealed partial class ChatView : UserControl
    {
        public event EventHandler<(int UserId, int RequestId, int MessageId)>? ProceedToPaymentRequested;

        private ChatViewModel chatViewModel;
        private string? pendingImageFileName = null;

        public ChatViewModel ViewModel
        {
            get => this.chatViewModel;
            set
            {
                if (this.chatViewModel != null)
                {
                    this.chatViewModel.Messages.CollectionChanged -= this.OnMessagesChanged;
                    this.chatViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
                }

                this.chatViewModel = value;

                if (this.chatViewModel != null)
                {
                    this.chatViewModel.Messages.CollectionChanged += this.OnMessagesChanged;
                    this.chatViewModel.PropertyChanged += this.OnViewModelPropertyChanged;

                    this.BannerDisplayName.Text = this.chatViewModel.DisplayName;
                    this.SetupAvatar();

                    this.RefreshMessages();
                }
            }
        }

        public int CurrentUserId { get; set; }

        public ChatView()
        {
            this.InitializeComponent();
        }

        private void RefreshMessages()
        {
            this.MessagesPanel.Children.Clear();
            foreach (var messageViewModel in this.chatViewModel.Messages)
            {
                var itemView = new MessageItemView();
                itemView.SetMessage(messageViewModel, this.CurrentUserId);

                itemView.AcceptRequested += this.OnAcceptRequested;
                itemView.DeclineRequested += this.OnDeclineRequested;
                itemView.CancelRequested += this.OnCancelRequested;
                itemView.AgreementAccepted += this.OnAcceptCashAgreement;
                itemView.ProceedToPaymentRequested += (sender, paymentArguments) => this.ProceedToPaymentRequested?.Invoke(sender, paymentArguments);

                this.MessagesPanel.Children.Add(itemView);
            }

            this.ScrollToBottom();
        }

        private void OnMessagesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs collectionChangedEventArgs)
        {
            if (collectionChangedEventArgs.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (MessageViewModel addedMessageViewModel in collectionChangedEventArgs.NewItems)
                {
                    var itemView = new MessageItemView();
                    itemView.SetMessage(addedMessageViewModel, this.CurrentUserId);

                    itemView.AcceptRequested += this.OnAcceptRequested;
                    itemView.DeclineRequested += this.OnDeclineRequested;
                    itemView.CancelRequested += this.OnCancelRequested;
                    itemView.AgreementAccepted += this.OnAcceptCashAgreement;
                    itemView.ProceedToPaymentRequested += (eventSender, paymentArguments) => this.ProceedToPaymentRequested?.Invoke(eventSender, paymentArguments);

                    this.MessagesPanel.Children.Add(itemView);
                }
            }
            else
            {
                this.RefreshMessages();
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(ChatViewModel.DisplayName))
            {
                this.BannerDisplayName.Text = this.chatViewModel.DisplayName;
            }
            else if (propertyChangedEventArgs.PropertyName == nameof(ChatViewModel.InputText))
            {
                this.MessageInput.Text = this.chatViewModel.InputText;
            }
            else if (propertyChangedEventArgs.PropertyName == nameof(ChatViewModel.AvatarUrl))
            {
                this.SetupAvatar();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.pendingImageFileName != null)
            {
                this.ViewModel?.SendImage(this.pendingImageFileName);
                this.ClearPendingImage();
            }
            else if (!string.IsNullOrWhiteSpace(this.ViewModel?.InputText))
            {
                this.ViewModel?.SendMessage();
            }
        }

        private void SetupAvatar()
        {
            this.AvatarPicture.DisplayName = this.chatViewModel.DisplayName;
            if (!string.IsNullOrEmpty(this.chatViewModel.AvatarUrl))
            {
                try
                {
                    string fullPath = Path.Combine(AppContext.BaseDirectory, "Images", this.chatViewModel.AvatarUrl);
                    this.AvatarPicture.ProfilePicture = new BitmapImage(new Uri(fullPath));
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Error loading avatar: {exception.Message}");
                    this.AvatarPicture.ProfilePicture = null;
                }
            }
            else
            {
                this.AvatarPicture.ProfilePicture = null;
            }
        }

        private void ScrollToBottom()
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                this.ScrollContainer.UpdateLayout();
                this.ScrollContainer.ChangeView(null, this.ScrollContainer.ExtentHeight, null);
            });
        }

        private async void MessageInput_Paste(object sender, TextControlPasteEventArgs pasteEventArgs)
        {
            var clipboardData = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            var clipboardFormats = clipboardData.AvailableFormats;
            System.Diagnostics.Debug.WriteLine("Clipboard formats: " + string.Join(", ", clipboardFormats));

            if (clipboardData.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Bitmap))
            {
                pasteEventArgs.Handled = true;

                var bitmapStreamReference = await clipboardData.GetBitmapAsync();
                var rawStream = await bitmapStreamReference.OpenReadAsync();

                var bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(rawStream);
                this.ImagePreview.Source = bitmapImage;
                this.ImagePreviewPanel.Visibility = Visibility.Visible;

                rawStream.Seek(0);

                string generatedFileName = $"{Guid.NewGuid()}.jpg";
                string fullImagePath = Path.Combine(AppContext.BaseDirectory, "Images", generatedFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(fullImagePath));

                using var fileStream = File.Create(fullImagePath);
                await rawStream.AsStreamForRead().CopyToAsync(fileStream);

                this.pendingImageFileName = generatedFileName;
            }
        }

        private void ClearPendingImage()
        {
            this.pendingImageFileName = null;
            this.ImagePreview.Source = null;
            this.ImagePreviewPanel.Visibility = Visibility.Collapsed;
        }

        private void RemoveImageButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            this.ClearPendingImage();
        }

        private async void AttachImageButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(
                (Application.Current as BookingBoardGames.App)?.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file == null) return;

            string generatedFileName = $"{Guid.NewGuid()}{file.FileType}";
            string fullImagePath = Path.Combine(AppContext.BaseDirectory, "Images", generatedFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(fullImagePath));

            using var rawStream = await file.OpenReadAsync();

            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(rawStream);
            this.ImagePreview.Source = bitmapImage;
            this.ImagePreviewPanel.Visibility = Visibility.Visible;

            rawStream.Seek(0);

            using var fileStream = File.Create(fullImagePath);
            await rawStream.AsStreamForRead().CopyToAsync(fileStream);

            this.pendingImageFileName = generatedFileName;
        }

        private void OnAcceptRequested(object? sender, int messageId)
        {
            this.ViewModel?.ResolveBookingRequest(messageId, true);
        }

        private void OnDeclineRequested(object? sender, int messageId)
        {
            this.ViewModel?.ResolveBookingRequest(messageId, false);
        }

        private void OnCancelRequested(object? sender, int messageId)
        {
            this.ViewModel?.ResolveBookingRequest(messageId, false);
        }

        private void OnAcceptCashAgreement(object? sender, int messageId)
        {
            this.ViewModel?.UpdateCashAgreement(messageId);
        }
    }
}
