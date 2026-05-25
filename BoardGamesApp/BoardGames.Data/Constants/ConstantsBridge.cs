namespace BoardGames.Data.Constants
{
    public static class App
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

    public static class ValidationMessages
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
