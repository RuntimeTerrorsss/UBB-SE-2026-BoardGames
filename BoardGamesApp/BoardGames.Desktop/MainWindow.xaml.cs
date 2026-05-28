using Microsoft.UI.Xaml;

namespace BoardGames.Desktop
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Closed += this.MainWindow_Closed;
        }

        public void SetRootContent(UIElement rootContent)
        {
            this.Content = rootContent;
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            Environment.Exit(0);
        }
    }
}
