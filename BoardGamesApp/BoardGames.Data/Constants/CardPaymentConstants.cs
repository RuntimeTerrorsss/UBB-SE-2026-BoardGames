// <copyright file="CardPaymentConstants.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;

namespace BookingBoardGames.Data.Constants
{
    public class CardPaymentConstants : PaymentConstrants
    {
        public const int NullBalance = 0;
        public const int NullPrice = 0;
        public const double TimerBeforeClosingPayment = 30_000;
        public const double TimerForRefreshingBalance = 4000;
        public const int LoadingTime = 50;
        public const int SuccessfulPaymentState = 1;
        public const string CardPaymentMethodName = "CARD";
    }
}
