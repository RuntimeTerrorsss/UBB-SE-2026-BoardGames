using BoardGames.Shared.DTO;

namespace BoardGames.Api.Legacy.Mappers
{
    public interface ICashPaymentMapper
    {
        public Payment TurnDTOIntoEntity(CashPaymentDTO paymentDto);

        public CashPaymentDTO TurnEntityIntoDTO(Payment payment);
    }
}
