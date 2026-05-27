// <copyright file="ICashPaymentMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>


// <copyright file="ICashPaymentMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Api.Mappers
{
    public interface ICashPaymentMapper
    {
        public Payment TurnDataTransferObjectIntoEntity(CashPaymentDataTransferObject paymentDto);

        public CashPaymentDataTransferObject TurnEntityIntoDataTransferObject(Payment payment);
    }
}
