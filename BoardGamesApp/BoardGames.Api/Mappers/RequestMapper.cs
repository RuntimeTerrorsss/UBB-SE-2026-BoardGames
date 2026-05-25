// <copyright file="RequestMapper.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;
using BoardGames.Shared.DTO;

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
                Game = this.gameMapper.ToDTO(request.Game),
                Renter = this.participantMapper.ToDTO(request.Renter),
                Owner = this.participantMapper.ToDTO(request.Owner),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status,
                OfferingUser = request.OfferingUser != null ? this.participantMapper.ToDTO(request.OfferingUser) : null,
            };
        }

        public Request? ToModel(RequestDTO? dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new Request
            {
                Id = dto.Id,
                Game = this.gameMapper.ToModel(dto.Game),
                Renter = this.participantMapper.ToModel(dto.Renter),
                Owner = this.participantMapper.ToModel(dto.Owner),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                OfferingUser = dto.OfferingUser != null ? this.participantMapper.ToModel(dto.OfferingUser) : null,
            };
        }
    }
}
