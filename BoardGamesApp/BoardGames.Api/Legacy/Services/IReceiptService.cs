namespace BoardGames.Api.Legacy.Services
{
    public interface IReceiptService
    {
        public string GenerateReceiptRelativePath(int rentalId);

        public Task<string> GetReceiptDocument(Payment payment);
    }
}
