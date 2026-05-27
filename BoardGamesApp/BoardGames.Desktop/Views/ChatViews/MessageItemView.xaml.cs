// <copyright file="MessageItemView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Desktop.ViewModels;

namespace BoardGames.Desktop.Views.ChatViews
{
    public sealed partial class MessageItemView : UserControl
    {
        private TextBlock statusIcon;

        public ChatViewModel ViewModel { get; set; }

        public event EventHandler<int>? AcceptRequested;

        public event EventHandler<int>? DeclineRequested;

        public event EventHandler<int>? CancelRequested;

        public event EventHandler<int>? AgreementAccepted;

        public event EventHandler<(int userId, int requestId, int messageId)>? ProceedToPaymentRequested;

        public MessageItemView()
        {
            this.InitializeComponent();
        }

        public void MarkAsRead()
        {
            if (this.statusIcon != null)
            {
                this.statusIcon.Text = "\uE73E\uE73E";
            }
        }

        private TextBlock CreateStatusIcon(bool isRead)
        {
            double smallIconFontSize = 11;
            double topMargin = 1;
            double rightMargin = 2;

            this.statusIcon = new TextBlock
            {
                Text = isRead ? "\uE73E\uE73E" : "\uE73E",
                Margin = new Thickness(0, topMargin, rightMargin, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"],
                FontSize = smallIconFontSize,
                Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"],
            };
            return this.statusIcon;
        }

        public void SetMessage(MessageViewModel message, int currentUserId)
        {
            switch (message.Type)
            {
                case MessageType.MessageSystem:
                    this.RenderSystemMessage(message);
                    break;

                case MessageType.MessageText:
                    this.RenderTextMessage(message, currentUserId);
                    break;

                case MessageType.MessageImage:
                    this.RenderImageMessage(message, currentUserId);
                    break;

                case MessageType.MessageRentalRequest:
                    this.RenderBookingRequest(message, currentUserId);
                    break;

                case MessageType.MessageCashAgreement:
                    this.RenderCashAgreement(message, currentUserId);
                    break;
            }
        }

        private void RenderSystemMessage(MessageViewModel message)
        {
            double systemMessageFontSize = 11;
            double topMargin = 4;
            double bottomMargin = 8;

            var textBlock = new TextBlock
            {
                Margin = new Thickness(0, topMargin, 0, bottomMargin),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = systemMessageFontSize,
                Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"],
                TextWrapping = TextWrapping.Wrap,
            };
            foreach (var messagePart in Regex.Split(message.Content, @"(\S+\.pdf)"))
            {
                var currentPath = messagePart;
                if (currentPath.EndsWith(".pdf"))
                {
                    var fileHyperlink = new Hyperlink();
                    fileHyperlink.Inlines.Add(new Run { Text = "File" });
                    fileHyperlink.Click += (sender, routedEventArgs) => System.Diagnostics.Process.Start("explorer.exe", currentPath);
                    textBlock.Inlines.Add(fileHyperlink);
                }
                else
                {
                    textBlock.Inlines.Add(new Run { Text = currentPath });
                }
            }

            this.MessagePresenter.Content = textBlock;
        }

        private void RenderTextMessage(MessageViewModel message, int currentUserId)
        {
            double maximumBubbleWidth = 480;
            double itemSpacing = 2;
            double horizontalPadding = 12;
            double verticalPadding = 8;
            double standardBorderThickness = 1;
            double curvedCornerRadius = 12;
            double flatCornerRadius = 2;

            bool isMine = message.SenderId == currentUserId;

            var stackPanel = new StackPanel
            {
                MaxWidth = maximumBubbleWidth,
                HorizontalAlignment = isMine ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Spacing = itemSpacing,
            };

            var border = new Border
            {
                Padding = new Thickness(horizontalPadding, verticalPadding, horizontalPadding, verticalPadding),
                CornerRadius = isMine ? new CornerRadius(curvedCornerRadius, curvedCornerRadius, flatCornerRadius, curvedCornerRadius) : new CornerRadius(curvedCornerRadius, curvedCornerRadius, curvedCornerRadius, flatCornerRadius),
            };

            var textBlock = new TextBlock { TextWrapping = TextWrapping.Wrap };
            foreach (var messagePart in Regex.Split(message.Content, @"(https://\S+)"))
            {
                if (messagePart.StartsWith("https://") && Uri.TryCreate(messagePart, UriKind.Absolute, out var validatedUri))
                {
                    var linkHyperlink = new Hyperlink { NavigateUri = validatedUri };
                    linkHyperlink.Inlines.Add(new Run { Text = messagePart });
                    textBlock.Inlines.Add(linkHyperlink);
                }
                else
                {
                    textBlock.Inlines.Add(new Run { Text = messagePart });
                }
            }

            if (isMine)
            {
                border.Background = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
                textBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
            }
            else
            {
                border.Background = (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"];
                border.BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"];
                border.BorderThickness = new Thickness(standardBorderThickness);
                textBlock.Foreground = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            }

            border.Child = textBlock;
            stackPanel.Children.Add(border);

            if (isMine)
            {
                stackPanel.Children.Add(this.CreateStatusIcon(message.IsRead));
            }

            this.MessagePresenter.Content = stackPanel;
        }

        private void RenderImageMessage(MessageViewModel message, int currentUserId)
        {
            double maximumImagePanelWidth = 320;
            double imagePreviewWidth = 280;
            double imagePreviewHeight = 180;
            double itemSpacing = 2;
            double outerPadding = 4;
            double standardBorderThickness = 1;
            double innerCornerRadius = 8;
            double curvedCornerRadius = 12;
            double flatCornerRadius = 2;

            bool isMine = message.SenderId == currentUserId;

            var stackPanel = new StackPanel
            {
                MaxWidth = maximumImagePanelWidth,
                HorizontalAlignment = isMine ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Spacing = itemSpacing,
            };

            var outerBorder = new Border
            {
                Padding = new Thickness(outerPadding),
                CornerRadius = isMine ? new CornerRadius(curvedCornerRadius, curvedCornerRadius, flatCornerRadius, curvedCornerRadius) : new CornerRadius(curvedCornerRadius, curvedCornerRadius, curvedCornerRadius, flatCornerRadius),
            };

            if (isMine)
            {
                outerBorder.Background = (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
            }
            else
            {
                outerBorder.Background = (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"];
                outerBorder.BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"];
                outerBorder.BorderThickness = new Thickness(standardBorderThickness);
            }

            var imageBorder = new Border
            {
                Width = imagePreviewWidth,
                Height = imagePreviewHeight,
                Background = (Brush)Application.Current.Resources["SubtleFillColorSecondaryBrush"],
                CornerRadius = new CornerRadius(innerCornerRadius),
            };

            if (message.ImageUrl != null && message.ImageUrl.Length > 0)
            {
                var chatImage = new Image
                {
                    Stretch = Stretch.UniformToFill,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                try
                {
                    string fullImagePath = Path.Combine(AppContext.BaseDirectory, "Images", message.ImageUrl);
                    var loadedBitmap = new BitmapImage(new Uri(fullImagePath));

                    chatImage.Source = loadedBitmap;
                    imageBorder.Child = chatImage;
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading image: {exception.Message}");
                    imageBorder.Child = this.CreateImagePlaceholder();
                }
            }
            else
            {
                imageBorder.Child = this.CreateImagePlaceholder();
            }

            outerBorder.Child = imageBorder;
            stackPanel.Children.Add(outerBorder);

            if (isMine)
            {
                stackPanel.Children.Add(this.CreateStatusIcon(message.IsRead));
            }

            this.MessagePresenter.Content = stackPanel;
        }

        private StackPanel CreateImagePlaceholder()
        {
            double iconFontSize = 32;
            double textFontSize = 12;
            double elementSpacing = 6;

            var placeholderPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = elementSpacing,
            };

            var placeholderIcon = new FontIcon
            {
                Glyph = "\uEB9F",
                FontSize = iconFontSize,
                Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"],
            };

            var placeholderText = new TextBlock
            {
                Text = "Image",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = textFontSize,
                Foreground = (Brush)Application.Current.Resources["TextFillColorTertiaryBrush"],
            };

            placeholderPanel.Children.Add(placeholderIcon);
            placeholderPanel.Children.Add(placeholderText);

            return placeholderPanel;
        }

        private void RenderBookingRequest(MessageViewModel message, int currentUserId)
        {
            double maximumRequestWidth = 400;
            double verticalMargin = 10;
            double horizontalPadding = 16;
            double verticalPadding = 12;
            double standardBorderThickness = 1;
            double outerCornerRadius = 10;
            double panelSpacing = 10;
            double titleFontSize = 13;
            double contentFontSize = 12;
            double buttonSpacing = 8;
            double minimumButtonWidth = 90;

            var border = new Border
            {
                MaxWidth = maximumRequestWidth,
                Margin = new Thickness(0, verticalMargin, 0, verticalMargin),
                Padding = new Thickness(horizontalPadding, verticalPadding, horizontalPadding, verticalPadding),
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                BorderThickness = new Thickness(standardBorderThickness),
                CornerRadius = new CornerRadius(outerCornerRadius),
            };

            var stackPanel = new StackPanel { Spacing = panelSpacing };

            var titleTextBlock = new TextBlock
            {
                Text = "Rental Request",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = titleFontSize,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"],
            };

            var bodyTextBlock = new TextBlock
            {
                Text = message.Content,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = contentFontSize,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                TextWrapping = TextWrapping.Wrap,
            };

            stackPanel.Children.Add(titleTextBlock);
            stackPanel.Children.Add(bodyTextBlock);

            if ((!message.IsResolved && !message.IsAccepted) && message.SenderId != currentUserId)
            {
                var actionButtonPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Orientation = Orientation.Horizontal,
                    Spacing = buttonSpacing,
                };

                var acceptActionButton = new Button
                {
                    Content = "Accept",
                    MinWidth = minimumButtonWidth,
                    Style = (Style)Application.Current.Resources["AccentButtonStyle"],
                };

                var declineActionButton = new Button
                {
                    Content = "Decline",
                    MinWidth = minimumButtonWidth,
                };

                acceptActionButton.Click += (sender, routedEventArgs) => this.AcceptRequested?.Invoke(this, message.Id);
                declineActionButton.Click += (sender, routedEventArgs) => this.DeclineRequested?.Invoke(this, message.Id);

                actionButtonPanel.Children.Add(acceptActionButton);
                actionButtonPanel.Children.Add(declineActionButton);
                stackPanel.Children.Add(actionButtonPanel);
            }
            else if (message.SenderId == currentUserId && !message.IsResolved)
            {
                if (!message.IsAccepted)
                {
                    var cancelPanel = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Orientation = Orientation.Horizontal,
                        Spacing = buttonSpacing,
                    };

                    var cancelRequestButton = new Button
                    {
                        Content = "Cancel",
                        MinWidth = minimumButtonWidth,
                    };

                    cancelRequestButton.Click += (sender, routedEventArgs) => this.CancelRequested?.Invoke(this, message.Id);

                    cancelPanel.Children.Add(cancelRequestButton);
                    stackPanel.Children.Add(cancelPanel);
                }
                else
                {
                    var paymentPanel = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Orientation = Orientation.Horizontal,
                        Spacing = buttonSpacing,
                    };

                    var proceedPaymentButton = new Button
                    {
                        Content = "Proceed to payment",
                        MinWidth = minimumButtonWidth,
                    };

                    proceedPaymentButton.Click += (sender, routedEventArgs) => this.ProceedToPaymentRequested?.Invoke(this, (currentUserId, message.RequestId, message.Id));

                    paymentPanel.Children.Add(proceedPaymentButton);
                    stackPanel.Children.Add(paymentPanel);
                }
            }

            border.Child = stackPanel;
            this.MessagePresenter.Content = border;
        }

        private void RenderCashAgreement(MessageViewModel message, int currentUserId)
        {
            double maximumAgreementWidth = 380;
            double verticalMargin = 4;
            double horizontalPadding = 14;
            double verticalPadding = 10;
            double standardBorderThickness = 1;
            double outerCornerRadius = 10;
            double panelSpacing = 8;
            double titleFontSize = 13;
            double contentFontSize = 12;

            var border = new Border
            {
                MaxWidth = maximumAgreementWidth,
                Margin = new Thickness(0, verticalMargin, 0, verticalMargin),
                Padding = new Thickness(horizontalPadding, verticalPadding, horizontalPadding, verticalPadding),
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                BorderThickness = new Thickness(standardBorderThickness),
                CornerRadius = new CornerRadius(outerCornerRadius),
            };

            var stackPanel = new StackPanel { Spacing = panelSpacing };

            var titleTextBlock = new TextBlock
            {
                Text = "Cash Transaction Agreement",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = titleFontSize,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"],
            };

            var bodyTextBlock = new TextBlock
            {
                Text = message.Content,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = contentFontSize,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                TextWrapping = TextWrapping.Wrap,
            };

            stackPanel.Children.Add(titleTextBlock);
            stackPanel.Children.Add(bodyTextBlock);

            bool currentUserAccepted = message.AcceptedBy != null && message.AcceptedBy.Contains(currentUserId);
            bool isSeller = message.SenderId == currentUserId;

            if (!currentUserAccepted)
            {
                var confirmAgreementButton = new Button
                {
                    Content = isSeller ? "I received the cash payment" : "I received the boardgame",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Style = (Style)Application.Current.Resources["AccentButtonStyle"],
                };
                confirmAgreementButton.Click += (sender, routedEventArgs) => this.AgreementAccepted(this, message.Id);
                stackPanel.Children.Add(confirmAgreementButton);
            }

            border.Child = stackPanel;
            this.MessagePresenter.Content = border;
        }
    }
}
