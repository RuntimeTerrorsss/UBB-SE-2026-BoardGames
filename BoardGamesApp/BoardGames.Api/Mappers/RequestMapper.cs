using BoardGames.Data.Models;
using BoardGames.Shared.DTO;
using DataRequestStatus = BoardGames.Data.Enums.RequestStatus;
using DtoRequestStatus = BoardGames.Shared.DTO.RequestStatus;

namespace BoardGames.Api.Mappers
{
    public class RequestMapper
    {
        private readonly GameMapper gameMapper;
        private readonly UserMapper participantMapper;

        public RequestMapper(GameMapper gameMapper, UserMapper participantMapper)
        {
            this.gameMapper = gameMapper;
            this.participantMapper = participantMapper;
        }

        public RequestDTO? ToDTO(Request? request)
        {
            if (request == null)
            {
                return null;
            }

            return new RequestDTO
            {
                Id = request.Id,
                Game = gameMapper.ToSummaryDTO(request.Game),
                Renter = participantMapper.ToDTO(request.Renter),
                Owner = participantMapper.ToDTO(request.Owner),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = ToDataTransferObjectStatus(request.Status),
                OfferingUser = request.OfferingUser != null ? participantMapper.ToDTO(request.OfferingUser) : null,
            };
        }

        public static DtoRequestStatus ToDataTransferObjectStatus(DataRequestStatus requestStatus) =>
            (DtoRequestStatus)(int)requestStatus;

        public static DataRequestStatus ToModelStatus(DtoRequestStatus requestStatus) =>
            (DataRequestStatus)(int)requestStatus;
    }
}
