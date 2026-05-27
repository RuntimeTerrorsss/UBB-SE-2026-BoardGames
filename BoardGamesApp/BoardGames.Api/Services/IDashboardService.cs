using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public interface IDashboardService
    {
        Task<List<PaymentDTO>> GetPaymentHistoryForUser(Guid accountId);
    }
}
