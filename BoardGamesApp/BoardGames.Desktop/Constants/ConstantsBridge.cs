// <copyright file="ConstantsBridge.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.Constants
{
    public static class DialogTitles
    {
        public const string ValidationError = Constants.DialogTitles.ValidationError;
        public const string RequestFailed = Constants.DialogTitles.RequestFailed;
        public const string RentalFailed = Constants.DialogTitles.RentalFailed;
        public const string ApproveFailed = Constants.DialogTitles.ApproveFailed;
        public const string DeclineFailed = Constants.DialogTitles.DeclineFailed;
        public const string OfferFailed = Constants.DialogTitles.OfferFailed;
        public const string OfferGameConfirmation = Constants.DialogTitles.OfferGameConfirmation;
        public const string ApproveRequestConfirmation = Constants.DialogTitles.ApproveRequestConfirmation;
        public const string DeclineRequestConfirmation = Constants.DialogTitles.DeclineRequestConfirmation;
        public const string CancelRequestConfirmation = Constants.DialogTitles.CancelRequestConfirmation;
        public const string DeleteGameConfirmation = Constants.DialogTitles.DeleteGameConfirmation;
        public const string GameRemoved = Constants.DialogTitles.GameRemoved;
        public const string CannotDeleteGame = Constants.DialogTitles.CannotDeleteGame;
    }

    public static class DialogButtons
    {
        public const string Ok = Constants.DialogButtons.Ok;
        public const string Cancel = Constants.DialogButtons.Cancel;
        public const string GoBack = Constants.DialogButtons.GoBack;
        public const string Approve = Constants.DialogButtons.Approve;
        public const string Decline = Constants.DialogButtons.Decline;
        public const string Delete = Constants.DialogButtons.Delete;
        public const string CancelRequest = Constants.DialogButtons.CancelRequest;
        public const string Offer = Constants.DialogButtons.Offer;
    }

    public static class DialogMessages
    {
        public const string UnexpectedErrorOccurred = Constants.DialogMessages.UnexpectedErrorOccurred;
        public const string NoReasonProvided = Constants.DialogMessages.NoReasonProvided;
        public const string CreateRequestValidationError = Constants.DialogMessages.CreateRequestValidationError;
        public const string CreateRentalValidationError = Constants.DialogMessages.CreateRentalValidationError;
    }
}
