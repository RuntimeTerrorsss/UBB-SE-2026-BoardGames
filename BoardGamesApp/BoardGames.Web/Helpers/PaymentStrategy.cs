// <copyright file="PaymentStrategy.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Web.Helpers
{
    public interface IPaymentStrategy
    {
        string GetDisplayLabel();

        string FormatSummary(PaymentDTO payment);
    }

    public sealed class CardPaymentStrategy : IPaymentStrategy
    {
        public string GetDisplayLabel() => "Card";

        public string FormatSummary(PaymentDTO payment)
        {
            return $"{payment.AmountText} paid by card on {payment.DateText}";
        }
    }

    public sealed class CashPaymentStrategy : IPaymentStrategy
    {
        public string GetDisplayLabel() => "Cash";

        public string FormatSummary(PaymentDTO payment)
        {
            return $"{payment.AmountText} paid in cash — {payment.Status}";
        }
    }

    public static class PaymentStrategyFactory
    {
        private static readonly CardPaymentStrategy Card = new();
        private static readonly CashPaymentStrategy Cash = new();

        public static IPaymentStrategy GetStrategy(string? paymentMethod)
        {
            return string.Equals(paymentMethod, "CARD", StringComparison.OrdinalIgnoreCase)
                ? Card
                : Cash;
        }
    }
}
