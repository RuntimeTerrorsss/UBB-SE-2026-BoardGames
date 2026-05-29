
namespace BoardGames.Desktop.Navigation
{
    public class BookingNavigationArguments
    {
        public int RequestIdentifier { get; set; }

        public required string DeliveryAddress { get; set; }

        public int BookingMessageIdentifier { get; set; }

        public required ConversationService ConversationService { get; set; }

        public required Window CurrentWindow { get; set; }
    }
}
