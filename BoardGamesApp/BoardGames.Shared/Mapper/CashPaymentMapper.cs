// <copyright file="CashPaymentMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using BoardGames.Shared.DTO;

namespace BoardGames.Shared.Mapper
{
    public class CashPaymentMapper : ICashPaymentMapper
    {
        private const string CashPaymentMethod = "CASH";

        public Payment TurnDataTransferObjectIntoEntity(CashPaymentDataTransferObject paymentDto)
        {
            return new Payment
            {
                RequestId = paymentDto.RequestId,
                ClientId = paymentDto.ClientId,
                OwnerId = paymentDto.OwnerId,
                PaidAmount = paymentDto.PaidAmount,
                PaymentMethod = CashPaymentMethod,
                DateOfTransaction = DateTime.Now,
            };
        }

        public CashPaymentDataTransferObject TurnEntityIntoDataTransferObject(Payment payment)
        {
            return new CashPaymentDataTransferObject(payment.TransactionIdentifier, payment.RequestId, payment.ClientId, payment.OwnerId, payment.PaidAmount);
        }
    }
}
