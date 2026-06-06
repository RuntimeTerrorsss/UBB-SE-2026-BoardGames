// <copyright file="DashboardView.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using BoardGames.Desktop.ViewModels;

namespace BoardGames.Desktop.Views
{
    public sealed partial class DashboardView : Page
    {
        public DashboardView()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<DashboardViewModel>();
        }
    }
}
