// <copyright file="RepositoryPayment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using BoardGames.Data.Constants;
using Microsoft.EntityFrameworkCore;

namespace BoardGames.Data.Repositories
{
    public class RepositoryPayment : IRepositoryPayment
    {
        private readonly AppDbContext context;

        public RepositoryPayment(AppDbContext appContext)
        {
            context = appContext;
        }

        public async Task<IReadOnlyList<HistoryPayment>> GetAllPayments()
        {
            return await BuildPaymentQuery().ToListAsync();
        }

        public async Task<HistoryPayment?> GetPaymentById(int searchedPaymentId)
        {
            return await BuildPaymentQuery()
                .FirstOrDefaultAsync(payment => payment.TransactionIdentifier == searchedPaymentId);
        }

        private IQueryable<HistoryPayment> BuildPaymentQuery()
        {
            return context.Payments
                .Include(payment => payment.Request)
                    .ThenInclude(rental => rental!.Game)
                .Include(payment => payment.Owner)
                .Include(payment => payment.Client)
                .Select(payment => new HistoryPayment
                {
                    TransactionIdentifier = payment.TransactionIdentifier,
                    PaidAmount = payment.PaidAmount,
                    PaymentMethod = payment.PaymentMethod,
                    DateOfTransaction = payment.DateOfTransaction,
                    DateConfirmedBuyer = payment.DateConfirmedBuyer,
                    DateConfirmedSeller = payment.DateConfirmedSeller,
                    PaymentState = payment.PaymentState,
                    ReceiptFilePath = payment.ReceiptFilePath,
                    RequestId = payment.RequestId,
                    ClientId = payment.ClientId,
                    OwnerId = payment.OwnerId,
                    RentalStartDate = payment.Request != null ? payment.Request.StartDate : null,
                    RentalEndDate = payment.Request != null ? payment.Request.EndDate : null,
                    GameName = payment.Request != null && payment.Request.Game != null
                                    ? payment.Request.Game.Name
                                    : PaymentHistoryConstants.NullGameNameDefaultValue,
                    OwnerName = payment.Owner != null
                                    ? payment.Owner.DisplayName
                                    : PaymentHistoryConstants.NullOwnerNameDefaultValue,
                    ClientName = payment.Client != null
                                    ? payment.Client.DisplayName
                                    : PaymentHistoryConstants.NullOwnerNameDefaultValue,
                });
        }
    }
}
