using BoardGames.Shared.DTO;

namespace BoardGames.Api.Legacy.Services
{
    public interface ICardPaymentService
    {
        Task<CardPaymentDTO> AddCardPayment(int requestIdentifier, int clientIdentifier, int ownerIdentifier, decimal amount);

        Task<bool> CheckBalanceSufficiency(int requestIdentifier, int clientIdentifier);

        Task<CardPaymentDTO?> GetCardPaymentAsync(int paymentIdentifier);

        Task<decimal> GetCurrentBalance(int clientIdentifier);

        Task ProcessPayment(int rentalIdentifier, int clientIdentifier, int ownerIdentifier);

        CardPaymentDTO ConvertToDTO(Payment cardPayment);

        Task<RentalDTO> GetRequestDTO(int rentalIdentifier);
    }
}
