// <copyright file="IServicePayment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;
using BookingBoardGames.Data.Enum;

namespace BoardGames.Shared.Services
{
    public interface IServicePayment
    {
        /// <summary>
        /// Retrieves all transactions without any filtering, mapped to DTOs for UI display.
        /// </summary>
        /// <returns>A list of all mapped TransactionDto objects.</returns>
        Task<List<PaymentDataTransferObject>> GetAllPaymentsForUI();

        /// <summary>
        /// Retrieves transactions mapped to DTOs, filtered or sorted by the given criteria and payment method,
        /// supporting pagination.
        /// </summary>
        /// <param name="filter">The chosen filter or sort type.</param>
        /// <param name="paymentMethod">The chosen payment method (ALL, CASH, CARD).</param>
        /// <param name="searchQuery">Text used to search by Product Name.</param>
        /// <param name="pageNumber">Current page index (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>A paginated result containing mapped TransactionDto objects.</returns>
        Task<PagedResult<PaymentDataTransferObject>> GetFilteredPayments(FilterType filter, PaymentMethod paymentMethod = PaymentMethod.ALL, string searchQuery = "", int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Computes the sum total amount from a given sequence of displayed transactions.
        /// </summary>
        /// <param name="displayedPayments">The collection of current interface transactions to sum up.</param>
        /// <returns>A decimal reflecting the total combined raw amount.</returns>
        decimal CalculateTotalAmount(IEnumerable<PaymentDataTransferObject> displayedPayments);

        /// <summary>
        /// Retrieves the full file path of the receipt, ensuring it exists.
        /// </summary>
        /// <param name="paymentId">The ID of the transaction.</param>
        /// <returns>The string file path to the Receipt PDF.</returns>
        Task<string> GetReceiptDocumentPath(int paymentId);

        /// <summary>
        /// Retrieves or generates a receipt PDF for a rental (paid or unpaid).
        /// </summary>
        Task<string> GetReceiptDocumentPathForRental(int rentalId);
    }
}
