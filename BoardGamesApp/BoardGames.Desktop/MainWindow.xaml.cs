using Microsoft.UI.Xaml;
using BoardGames.Desktop.Views;

namespace BoardGames.Desktop
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.RootFrame.Navigate(typeof(LoginPage));

            this.Closed += (s, e) => System.Environment.Exit(0);
        }
    }
}