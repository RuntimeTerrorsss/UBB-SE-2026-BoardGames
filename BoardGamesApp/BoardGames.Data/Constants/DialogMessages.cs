namespace BoardGames.Data.Constants
{
    public static class DialogMessages
    {
        public const string UnexpectedErrorOccurred = "An unexpected error occurred.";
        public const string NoReasonProvided = "No reason provided.";
        public const string CreateRequestValidationError =
            "Please select a game and valid date range (start date must be before end date and not in the past).";
        public const string CreateRentalValidationError =
            "Please select a game, a renter, and a valid date range (start before end, not in the past).";
    }
}
