namespace BoardGames.Api.Legacy.Services
{
    public interface IPaymentService
    {
        Task GenerateReceiptAsync(int paymentId);

        Task<string> GetReceiptAsync(int paymentId);
    }
}
