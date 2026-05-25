// <copyright file="IPaymentRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BoardGames.Data.Repositories
{
    /// <summary>
    /// Defines methods for retrieving payment common records from a data source.
    /// </summary>
    public interface IPaymentRepository
    {
        public Task<IReadOnlyList<Payment>> GetAllPaymentsAsync();

        public Task<Payment?> GetPaymentByIdentifierAsync(int paymentId);

        public Task<int> AddPaymentAsync(Payment payment);

        public Task<bool> DeletePaymentAsync(Payment payment);

        public Task<Payment?> UpdatePaymentAsync(Payment payment);
    }
}
