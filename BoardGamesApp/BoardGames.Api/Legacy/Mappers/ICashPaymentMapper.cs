// <copyright file="ICashPaymentMapper.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

// <copyright file="ICashPaymentMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;

namespace BoardGames.Api.Mappers
{
    public interface ICashPaymentMapper
    {
        public Payment TurnDTOIntoEntity(CashPaymentDTO paymentDto);

        public CashPaymentDTO TurnEntityIntoDTO(Payment payment);
    }
}
