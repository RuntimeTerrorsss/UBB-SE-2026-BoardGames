// <copyright file="PaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Data.Repositories;

namespace BoardGames.Api.Services
{
    public abstract class PaymentService : IPaymentService
    {
        protected readonly IPaymentRepository paymentRepository;
        protected readonly IReceiptService receiptService;

        public PaymentService(IPaymentRepository paymentRepository, IReceiptService receiptService)
        {
            this.receiptService = receiptService;
            this.paymentRepository = paymentRepository;
        }

        /// <summary>
        /// Set the receipt file path of a payment (when everything is confirmed).
        /// </summary>
        /// <param name="paymentId">of payment to set file path to</param>
        public async Task GenerateReceiptAsync(int paymentId)
        {
            Payment? paymentToUpdate =
                await paymentRepository.GetPaymentByIdentifierAsync(paymentId);

            if (paymentToUpdate == null)
            {
                return;
            }

            paymentToUpdate.ReceiptFilePath =
                receiptService.GenerateReceiptRelativePath(paymentToUpdate.RequestId);

            await paymentRepository.UpdatePaymentAsync(paymentToUpdate);
        }

        /// <summary>
        /// Get the full path to the saved receipt pdf.
        /// </summary>
        /// <param name="paymentId">of payment to get pdf path</param>
        /// <returns>full path to pdf</returns>
        public async Task<string> GetReceiptAsync(int paymentId)
        {
            Payment? paymentToRead =
                await paymentRepository.GetPaymentByIdentifierAsync(paymentId);

            if (paymentToRead == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(paymentToRead.ReceiptFilePath))
            {
                await GenerateReceiptAsync(paymentId);

                paymentToRead =
                    await paymentRepository.GetPaymentByIdentifierAsync(paymentId);

                if (paymentToRead == null)
                {
                    return string.Empty;
                }
            }

            return await receiptService.GetReceiptDocument(paymentToRead);
        }
    }
}
