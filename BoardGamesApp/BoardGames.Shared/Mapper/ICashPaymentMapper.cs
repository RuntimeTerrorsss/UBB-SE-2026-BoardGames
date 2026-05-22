// <copyright file="ICashPaymentMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Shared.Mapper
{
    public interface ICashPaymentMapper
    {
        public Payment TurnDataTransferObjectIntoEntity(CashPaymentDataTransferObject paymentDto);

        public CashPaymentDataTransferObject TurnEntityIntoDataTransferObject(Payment payment);
    }
}
