// <copyright file="MainWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Desktop.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace BoardGames.Desktop
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static int loggedInUserAlice = 2;
        public static int loggedInUserBob = 3;

        public MainWindow()
        {
            this.InitializeComponent();

            this.RootFrame.Navigate(typeof(DiscoveryView));

            this.Closed += this.MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            Environment.Exit(0);
        }
    }
}
