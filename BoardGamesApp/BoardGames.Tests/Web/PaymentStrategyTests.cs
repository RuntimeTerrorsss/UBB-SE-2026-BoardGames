// <copyright file="PaymentStrategyTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Web.Helpers;
using Xunit;

namespace BoardGames.Tests.Web
{
    public class PaymentStrategyTests
    {
        [Fact]
        public void CardPaymentStrategy_GetDisplayLabel_ReturnsCard()
        {
            var strategy = new CardPaymentStrategy();
            Assert.Equal("Card", strategy.GetDisplayLabel());
        }

        [Fact]
        public void CashPaymentStrategy_GetDisplayLabel_ReturnsCash()
        {
            var strategy = new CashPaymentStrategy();
            Assert.Equal("Cash", strategy.GetDisplayLabel());
        }

        [Fact]
        public void CardPaymentStrategy_FormatSummary_IncludesAmountAndDate()
        {
            var strategy = new CardPaymentStrategy();
            var payment = new PaymentDTO
            {
                Amount = 25.50m,
                DateText = "2026-01-15",
            };

            string summary = strategy.FormatSummary(payment);

            Assert.Contains("paid by card", summary);
            Assert.Contains("2026-01-15", summary);
        }

        [Fact]
        public void CashPaymentStrategy_FormatSummary_IncludesAmountAndStatus()
        {
            var strategy = new CashPaymentStrategy();
            var payment = new PaymentDTO
            {
                Amount = 10m,
                Status = "Confirmed",
            };

            string summary = strategy.FormatSummary(payment);

            Assert.Contains("paid in cash", summary);
            Assert.Contains("Confirmed", summary);
        }

        [Fact]
        public void PaymentStrategyFactory_CardMethod_ReturnsCardStrategy()
        {
            var strategy = PaymentStrategyFactory.GetStrategy("CARD");
            Assert.IsType<CardPaymentStrategy>(strategy);
        }

        [Fact]
        public void PaymentStrategyFactory_CardMethodLowercase_ReturnsCardStrategy()
        {
            var strategy = PaymentStrategyFactory.GetStrategy("card");
            Assert.IsType<CardPaymentStrategy>(strategy);
        }

        [Fact]
        public void PaymentStrategyFactory_CashMethod_ReturnsCashStrategy()
        {
            var strategy = PaymentStrategyFactory.GetStrategy("CASH");
            Assert.IsType<CashPaymentStrategy>(strategy);
        }

        [Fact]
        public void PaymentStrategyFactory_NullMethod_DefaultsToCash()
        {
            var strategy = PaymentStrategyFactory.GetStrategy(null);
            Assert.IsType<CashPaymentStrategy>(strategy);
        }

        [Fact]
        public void PaymentStrategyFactory_UnknownMethod_DefaultsToCash()
        {
            var strategy = PaymentStrategyFactory.GetStrategy("CRYPTO");
            Assert.IsType<CashPaymentStrategy>(strategy);
        }
    }
}
