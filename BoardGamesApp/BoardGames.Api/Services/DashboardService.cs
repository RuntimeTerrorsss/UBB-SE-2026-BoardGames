using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IRepositoryPayment historyRepository;
        private readonly IAccountRepository accountRepository;

        public DashboardService(IRepositoryPayment historyRepository, IAccountRepository accountRepository)
        {
            this.historyRepository = historyRepository;
            this.accountRepository = accountRepository;
        }

        public async Task<List<PaymentDTO>> GetPaymentHistoryForUser(Guid accountId)
        {
            var user = await accountRepository.GetByIdAsync(accountId);
            if (user is null)
            {
                return new List<PaymentDTO>();
            }

            int pamUserId = user.PamUserId;
            var allPayments = await historyRepository.GetAllPayments();

            return allPayments
                .Where(p => p.ClientId == pamUserId || p.OwnerId == pamUserId)
                .Select(p => MapToDTO(p, pamUserId))
                .OrderByDescending(p => p.SortDate)
                .ToList();
        }

        private static PaymentDTO MapToDTO(HistoryPayment payment, int pamUserId)
        {
            bool isRenter = payment.ClientId == pamUserId;
            string role = isRenter ? "Renter" : "Owner";
            string? otherPartyName = isRenter ? payment.OwnerName : payment.ClientName;

            string? period = null;
            DateTime sortDate = payment.DateOfTransaction ?? DateTime.MinValue;
            if (payment.RentalStartDate.HasValue && payment.RentalEndDate.HasValue)
            {
                period = $"{payment.RentalStartDate.Value:d} – {payment.RentalEndDate.Value:d}";
                sortDate = payment.RentalStartDate.Value;
            }

            return new PaymentDTO
            {
                PaymentId = payment.TransactionIdentifier,
                ProductName = payment.GameName,
                OtherPartyName = otherPartyName,
                Role = role,
                Amount = payment.PaidAmount,
                Period = period,
                SortDate = sortDate,
                RentalId = payment.RequestId,
                HasPayment = true,
                PaymentMethod = payment.PaymentMethod,
                FilePath = payment.ReceiptFilePath,
            };
        }
    }
}
