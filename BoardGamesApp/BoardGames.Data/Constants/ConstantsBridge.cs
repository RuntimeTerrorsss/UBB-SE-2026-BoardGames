namespace BoardGames.Data.Constants
{
    internal static class App
    {
        public const string AppTrayIconUri = global::BoardGames.Data.Constants.Constants.AppTrayIconUri;
    }

    internal static class DialogTitles
    {
        public const string ValidationError = global::BoardGames.Data.Constants.Constants.DialogTitles.ValidationError;
        public const string RequestFailed = global::BoardGames.Data.Constants.Constants.DialogTitles.RequestFailed;
        public const string RentalFailed = global::BoardGames.Data.Constants.Constants.DialogTitles.RentalFailed;
        public const string ApproveFailed = global::BoardGames.Data.Constants.Constants.DialogTitles.ApproveFailed;
        public const string DeclineFailed = global::BoardGames.Data.Constants.Constants.DialogTitles.DeclineFailed;
        public const string OfferFailed = global::BoardGames.Data.Constants.Constants.DialogTitles.OfferFailed;
        public const string OfferGameConfirmation = global::BoardGames.Data.Constants.Constants.DialogTitles.OfferGameConfirmation;
        public const string ApproveRequestConfirmation = global::BoardGames.Data.Constants.Constants.DialogTitles.ApproveRequestConfirmation;
        public const string DeclineRequestConfirmation = global::BoardGames.Data.Constants.Constants.DialogTitles.DeclineRequestConfirmation;
        public const string CancelRequestConfirmation = global::BoardGames.Data.Constants.Constants.DialogTitles.CancelRequestConfirmation;
        public const string DeleteGameConfirmation = global::BoardGames.Data.Constants.Constants.DialogTitles.DeleteGameConfirmation;
        public const string GameRemoved = global::BoardGames.Data.Constants.Constants.DialogTitles.GameRemoved;
        public const string CannotDeleteGame = global::BoardGames.Data.Constants.Constants.DialogTitles.CannotDeleteGame;
    }

    internal static class DialogButtons
    {
        public const string Ok = global::BoardGames.Data.Constants.Constants.DialogButtons.Ok;
        public const string Cancel = global::BoardGames.Data.Constants.Constants.DialogButtons.Cancel;
        public const string GoBack = global::BoardGames.Data.Constants.Constants.DialogButtons.GoBack;
        public const string Approve = global::BoardGames.Data.Constants.Constants.DialogButtons.Approve;
        public const string Decline = global::BoardGames.Data.Constants.Constants.DialogButtons.Decline;
        public const string Delete = global::BoardGames.Data.Constants.Constants.DialogButtons.Delete;
        public const string CancelRequest = global::BoardGames.Data.Constants.Constants.DialogButtons.CancelRequest;
        public const string Offer = global::BoardGames.Data.Constants.Constants.DialogButtons.Offer;
    }

    internal static class InternalDialogMessages
    {
        public const string UnexpectedErrorOccurred = global::BoardGames.Data.Constants.Constants.DialogMessages.UnexpectedErrorOccurred;
        public const string NoReasonProvided = global::BoardGames.Data.Constants.Constants.DialogMessages.NoReasonProvided;
        public const string CreateRequestValidationError = global::BoardGames.Data.Constants.Constants.DialogMessages.CreateRequestValidationError;
        public const string CreateRentalValidationError = global::BoardGames.Data.Constants.Constants.DialogMessages.CreateRentalValidationError;
    }

    internal static class InternalNotificationTitles
    {
        public const string UpcomingRentalReminder = global::BoardGames.Data.Constants.Constants.NotificationTitles.UpcomingRentalReminder;
        public const string BookingUnavailable = global::BoardGames.Data.Constants.Constants.NotificationTitles.BookingUnavailable;
        public const string RentalRequestDeclined = global::BoardGames.Data.Constants.Constants.NotificationTitles.RentalRequestDeclined;
        public const string RentalRequestCancelled = global::BoardGames.Data.Constants.Constants.NotificationTitles.RentalRequestCancelled;
        public const string RentalRequestApproved = global::BoardGames.Data.Constants.Constants.NotificationTitles.RentalRequestApproved;
        public const string OfferReceived = global::BoardGames.Data.Constants.Constants.NotificationTitles.OfferReceived;
        public const string OfferAccepted = global::BoardGames.Data.Constants.Constants.NotificationTitles.OfferAccepted;
        public const string RentalConfirmed = global::BoardGames.Data.Constants.Constants.NotificationTitles.RentalConfirmed;
        public const string OfferDenied = global::BoardGames.Data.Constants.Constants.NotificationTitles.OfferDenied;
        public const string OfferDeclined = global::BoardGames.Data.Constants.Constants.NotificationTitles.OfferDeclined;
    }

    internal static class ValidationMessages
    {
        public const string MaximumPlayerCountComparedToMinimum =
            global::BoardGames.Data.Constants.Constants.ValidationMessages.MaximumPlayerCountComparedToMinimum;

        public static string NameLengthRange(int minimumLength, int maximumLength) =>
            global::BoardGames.Data.Constants.Constants.ValidationMessages.NameLengthRange(minimumLength, maximumLength);

        public static string PriceMinimum(decimal minimumPrice) =>
            global::BoardGames.Data.Constants.Constants.ValidationMessages.PriceMinimum(minimumPrice);

        public static string MinimumPlayerCount(int minimumPlayers) =>
            global::BoardGames.Data.Constants.Constants.ValidationMessages.MinimumPlayerCount(minimumPlayers);

        public static string DescriptionLengthRange(int minimumLength, int maximumLength) =>
            global::BoardGames.Data.Constants.Constants.ValidationMessages.DescriptionLengthRange(minimumLength, maximumLength);
    }

    internal static class GameValidation
    {
        public const int MinimumNameLength = DomainConstants.GameMinimumNameLength;
        public const int MaximumNameLength = DomainConstants.GameMaximumNameLength;
        public const decimal MinimumAllowedPrice = DomainConstants.GameMinimumAllowedPrice;
        public const int MinimumPlayerCount = DomainConstants.GameMinimumPlayerCount;
        public const int MinimumDescriptionLength = DomainConstants.GameMinimumDescriptionLength;
        public const int MaximumDescriptionLength = DomainConstants.GameMaximumDescriptionLength;
        public const int DefaultMinimumPlayers = DomainConstants.GameDefaultMinimumPlayers;
        public const int DefaultMaximumPlayers = DomainConstants.GameDefaultMaximumPlayers;
    }
}
