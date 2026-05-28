// <copyright file="CashPaymentMapper.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Api.Legacy.Mappers
{
    public class CashPaymentMapper : ICashPaymentMapper
    {
        private const string CashPaymentMethod = "CASH";

        public Payment TurnDTOIntoEntity(CashPaymentDTO paymentDto)
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

        public CashPaymentDTO TurnEntityIntoDTO(Payment payment)
        {
            return new CashPaymentDTO(payment.TransactionIdentifier, payment.RequestId, payment.ClientId, payment.OwnerId, payment.PaidAmount);
        }
    }
}
