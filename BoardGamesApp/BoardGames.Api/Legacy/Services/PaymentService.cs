using BoardGames.Data.Repositories;

namespace BoardGames.Api.Legacy.Services
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
        public async Task GenerateReceiptAsync(int paymentId)
        {
            Payment? paymentToUpdate =
                await this.paymentRepository.GetPaymentByIdentifierAsync(paymentId);

            if (paymentToUpdate == null)
            {
                return;
            }

            paymentToUpdate.ReceiptFilePath =
                this.receiptService.GenerateReceiptRelativePath(paymentToUpdate.RequestId);

            await this.paymentRepository.UpdatePaymentAsync(paymentToUpdate);
        }
        public async Task<string> GetReceiptAsync(int paymentId)
        {
            Payment? paymentToRead =
                await this.paymentRepository.GetPaymentByIdentifierAsync(paymentId);

            if (paymentToRead == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(paymentToRead.ReceiptFilePath))
            {
                await this.GenerateReceiptAsync(paymentId);

                paymentToRead =
                    await this.paymentRepository.GetPaymentByIdentifierAsync(paymentId);

                if (paymentToRead == null)
                {
                    return string.Empty;
                }
            }

            return await this.receiptService.GetReceiptDocument(paymentToRead);
        }
    }
}
