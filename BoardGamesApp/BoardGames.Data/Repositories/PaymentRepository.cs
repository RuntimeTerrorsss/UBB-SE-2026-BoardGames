// <copyright file="PaymentRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Data;
using Microsoft.EntityFrameworkCore;

namespace BoardGames.Data.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext context;

        public PaymentRepository(AppDbContext appContext)
        {
            context = appContext;
        }

        public async Task<IReadOnlyList<Payment>> GetAllPaymentsAsync()
        {
            return await context.Payments.ToListAsync();
        }

        public virtual async Task<Payment?> GetPaymentByIdentifierAsync(int paymentId)
        {
            return await context.Payments.FirstOrDefaultAsync(payment => payment.TransactionIdentifier == paymentId);
        }

        public virtual async Task<int> AddPaymentAsync(Payment payment)
        {
            if (payment.DateOfTransaction == default)
            {
                payment.DateOfTransaction = DateTime.Now;
            }

            if (payment.TransactionIdentifier <= 0)
            {
                payment.TransactionIdentifier = 0;
            }

            payment.Request = null;
            payment.Client = null;
            payment.Owner = null;

            await context.Payments.AddAsync(payment);
            await context.SaveChangesAsync();

            return payment.TransactionIdentifier;
        }

        public async Task<bool> DeletePaymentAsync(Payment payment)
        {
            var paymentToDelete = await context.Payments.FindAsync(payment.TransactionIdentifier);
            if (paymentToDelete == null)
            {
                return false;
            }

            context.Payments.Remove(paymentToDelete);
            return await context.SaveChangesAsync() > 0;
        }

        public virtual async Task<Payment?> UpdatePaymentAsync(Payment payment)
        {
            var existingPayment = await context.Payments.FindAsync(payment.TransactionIdentifier);

            if (existingPayment == null)
            {
                return null;
            }

            var previousPayment = new Payment
            {
                TransactionIdentifier = existingPayment.TransactionIdentifier,
                RequestId = existingPayment.RequestId,
                ClientId = existingPayment.ClientId,
                OwnerId = existingPayment.OwnerId,
                PaidAmount = existingPayment.PaidAmount,
                ReceiptFilePath = existingPayment.ReceiptFilePath,
                DateOfTransaction = existingPayment.DateOfTransaction,
                DateConfirmedBuyer = existingPayment.DateConfirmedBuyer,
                DateConfirmedSeller = existingPayment.DateConfirmedSeller,
                PaymentMethod = existingPayment.PaymentMethod,
                PaymentState = existingPayment.PaymentState,
            };

            existingPayment.ReceiptFilePath = payment.ReceiptFilePath ?? string.Empty;
            existingPayment.DateOfTransaction = payment.DateOfTransaction ?? DateTime.Now;
            existingPayment.DateConfirmedBuyer = payment.DateConfirmedBuyer;
            existingPayment.DateConfirmedSeller = payment.DateConfirmedSeller;

            await context.SaveChangesAsync();

            return previousPayment;
        }
    }
}
