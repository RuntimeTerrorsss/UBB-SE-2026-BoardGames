// <copyright file="IRepositoryPayment.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

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
