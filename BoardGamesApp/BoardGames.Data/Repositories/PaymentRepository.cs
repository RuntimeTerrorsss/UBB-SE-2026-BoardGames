// <copyright file="PaymentRepository.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;

namespace BoardGames.Data.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext context;

        public PaymentRepository(AppDbContext appContext)
        {
            this.context = appContext;
        }

        public async Task<IReadOnlyList<Payment>> GetAllPaymentsAsync()
        {
            return await this.context.Payments.ToListAsync();
        }

        public virtual async Task<Payment?> GetPaymentByIdentifierAsync(int paymentId)
        {
            return await this.context.Payments.FirstOrDefaultAsync(payment => payment.TransactionIdentifier == paymentId);
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

            await this.context.Payments.AddAsync(payment);
            await this.context.SaveChangesAsync();

            return payment.TransactionIdentifier;
        }

        public async Task<bool> DeletePaymentAsync(Payment payment)
        {
            var paymentToDelete = await this.context.Payments.FindAsync(payment.TransactionIdentifier);
            if (paymentToDelete == null)
            {
                return false;
            }

            this.context.Payments.Remove(paymentToDelete);
            return await this.context.SaveChangesAsync() > 0;
        }

        public virtual async Task<Payment?> UpdatePaymentAsync(Payment payment)
        {
            var existingPayment = await this.context.Payments.FindAsync(payment.TransactionIdentifier);

            if (existingPayment == null)
            {
                return null;
            }

            existingPayment.ReceiptFilePath = payment.ReceiptFilePath ?? string.Empty;
            existingPayment.DateOfTransaction = payment.DateOfTransaction ?? DateTime.Now;
            existingPayment.DateConfirmedBuyer = payment.DateConfirmedBuyer;
            existingPayment.DateConfirmedSeller = payment.DateConfirmedSeller;
            existingPayment.PaidAmount = payment.PaidAmount;

            await this.context.SaveChangesAsync();

            return existingPayment;
        }
    }
}
