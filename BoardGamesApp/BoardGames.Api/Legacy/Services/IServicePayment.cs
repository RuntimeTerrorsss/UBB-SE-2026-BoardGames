using BoardGames.Data.Enums;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Legacy.Services
{
    public interface IServicePayment
    {
        Task<List<PaymentDTO>> GetAllPaymentsForUI();
        Task<PagedResult<PaymentDTO>> GetFilteredPayments(FilterType filter, PaymentMethod paymentMethod = PaymentMethod.ALL, string searchQuery = "", int pageNumber = 1, int pageSize = 10);
        decimal CalculateTotalAmount(IEnumerable<PaymentDTO> displayedPayments);
        Task<string> GetReceiptDocumentPath(int paymentId);
        Task<string> GetReceiptDocumentPathForRental(int rentalId);
    }
}
