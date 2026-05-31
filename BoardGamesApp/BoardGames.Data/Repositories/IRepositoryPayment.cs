// <copyright file="IRepositoryPayment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace BoardGames.Data.Repositories
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
