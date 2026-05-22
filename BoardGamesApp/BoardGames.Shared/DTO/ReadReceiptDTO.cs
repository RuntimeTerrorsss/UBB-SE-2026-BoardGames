// <copyright file="ReadReceiptDTO.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGames.Shared.DTO
{
    public record ReadReceiptDTO(
        int ConversationId,
        int ReaderId,
        int ReceiverId,
        DateTime ReceiptTimeStamp);
}
