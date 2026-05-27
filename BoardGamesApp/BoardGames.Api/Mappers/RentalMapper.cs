using BoardGames.Data.Models;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Mappers
{
    public class RentalMapper
    {
        private readonly GameMapper gameMapper;
        private readonly UserMapper participantMapper;

        public RentalMapper(GameMapper gameMapper, UserMapper participantMapper)
        {
            this.gameMapper = gameMapper;
            this.participantMapper = participantMapper;
        }

        public RentalDTO? ToDTO(Rental? rental)
        {
            if (rental == null)
            {
                return null;
            }

            return new RentalDTO
            {
                Id = rental.Id,
                Game = gameMapper.ToSummaryDTO(rental.Game),
                Renter = participantMapper.ToDTO(rental.Renter),
                Owner = participantMapper.ToDTO(rental.Owner),
                StartDate = rental.StartDate,
                EndDate = rental.EndDate,
            };
        }
    }
}
