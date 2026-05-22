// <copyright file="IRepositoryPayment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace BookingBoardGames.Data.Interfaces
{
    /// <summary>
    /// Defines methods for retrieving payment history records from a data source.
    /// </summary>
    public interface IRepositoryPayment
    {
        Task<IReadOnlyList<HistoryPayment>> GetAllPayments();

        Task<HistoryPayment?> GetPaymentById(int searchedPaymentId);
    }
}
