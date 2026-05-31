// <copyright file="DashboardView.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.Views
{
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.Extensions.DependencyInjection;
    using BoardGames.Desktop.ViewModels;

    public sealed partial class DashboardView : Page
    {
        public DashboardView()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<DashboardViewModel>();
        }
    }
}
