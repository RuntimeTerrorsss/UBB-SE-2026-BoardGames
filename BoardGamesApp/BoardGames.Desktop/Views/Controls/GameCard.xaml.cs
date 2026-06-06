
namespace BoardGames.Desktop.Views.Controls
{
    public sealed partial class GameCard : UserControl
    {
        public static readonly DependencyProperty GameIdProperty =
            DependencyProperty.Register(
                nameof(GameId),
                typeof(int),
                typeof(GameCard),
                new PropertyMetadata(0));
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(GameCard),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                nameof(ImageSource),
                typeof(ImageSource),
                typeof(GameCard),
                new PropertyMetadata(null));
        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register(
                nameof(Location),
                typeof(string),
                typeof(GameCard),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty PlayersTextProperty =
            DependencyProperty.Register(
                nameof(PlayersText),
                typeof(string),
                typeof(GameCard),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty PriceTextProperty =
            DependencyProperty.Register(
                nameof(PriceText),
                typeof(string),
                typeof(GameCard),
                new PropertyMetadata(string.Empty));
        public GameCard()
        {
            this.InitializeComponent();
        }
        public int GameId
        {
            get => (int)this.GetValue(GameIdProperty);
            set => this.SetValue(GameIdProperty, value);
        }
        public string Title
        {
            get => (string)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }
        public ImageSource ImageSource
        {
            get => (ImageSource)this.GetValue(ImageSourceProperty);
            set => this.SetValue(ImageSourceProperty, value);
        }
        public string Location
        {
            get => (string)this.GetValue(LocationProperty);
            set => this.SetValue(LocationProperty, value);
        }
        public string PlayersText
        {
            get => (string)this.GetValue(PlayersTextProperty);
            set => this.SetValue(PlayersTextProperty, value);
        }
        public string PriceText
        {
            get => (string)this.GetValue(PriceTextProperty);
            set => this.SetValue(PriceTextProperty, value);
        }
        private void OnCardClicked(object sender, RoutedEventArgs e)
        {
            if (this.Parent is FrameworkElement parent)
            {
                var frame = parent.XamlRoot.Content as Frame;
                frame?.Navigate(typeof(GameDetailsView), this.GameId);
            }
        }
    }
}
