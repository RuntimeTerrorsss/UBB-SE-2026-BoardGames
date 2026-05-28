namespace BoardGames.Desktop.Constants
{
    public static class Constants
    {
        public static class DialogTitles
        {
            public const string ValidationError = "Validation Error";
            public const string RequestFailed = "Request Failed";
            public const string RentalFailed = "Rental Failed";
            public const string ApproveFailed = "Approve Failed";
            public const string DeclineFailed = "Decline Failed";
            public const string OfferFailed = "Offer Failed";
            public const string OfferGameConfirmation = "Offer Game?";
            public const string ApproveRequestConfirmation = "Approve Request?";
            public const string DeclineRequestConfirmation = "Decline Request?";
            public const string CancelRequestConfirmation = "Cancel Request?";
            public const string DeleteGameConfirmation = "Delete Game?";
            public const string GameRemoved = "Game Removed";
            public const string CannotDeleteGame = "Cannot Delete Game";
        }

        public static class DialogButtons
        {
            public const string Ok = "OK";
            public const string Cancel = "Cancel";
            public const string GoBack = "Go Back";
            public const string Approve = "Approve";
            public const string Decline = "Decline";
            public const string Delete = "Delete";
            public const string CancelRequest = "Cancel Request";
            public const string Offer = "Offer";
        }

        public static class DialogMessages
        {
            public const string UnexpectedErrorOccurred = "An unexpected error occurred.";
            public const string NoReasonProvided = "No reason provided.";
            public const string CreateRequestValidationError =
                "Please select a game and valid date range (start date must be before end date and not in the past).";

            public const string CreateRentalValidationError =
                "Please select a game, a renter, and a valid date range (start before end, not in the past).";
        }

        public static class ValidationMessages
        {
            public static string NameLengthRange(int minimumLength, int maximumLength) =>
                $"Name must be between {minimumLength} and {maximumLength} characters.";

            public static string PriceMinimum(decimal minimumPrice) =>
                $"Price must be greater than or equal to {minimumPrice:0}.";

            public static string MinimumPlayerCount(int minimumPlayers) =>
                $"Minimum player count must be at least {minimumPlayers}.";

            public const string MaximumPlayerCountComparedToMinimum =
                "Maximum player count must be greater than or equal to minimum player count.";

            public static string DescriptionLengthRange(int minimumLength, int maximumLength) =>
                $"Description must be between {minimumLength} and {maximumLength} characters.";
        }
    }
}
