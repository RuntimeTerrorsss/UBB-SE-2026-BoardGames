// <copyright file="ServicePayment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Data.Constants;
using BoardGames.Data.Enums;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;

namespace BoardGames.Api.Services
{
    /// <summary>
    /// Service responsible for business logic, mapping, computing totals, and filtering transactions for the Payment History view.
    /// </summary>
    public class ServicePayment : IServicePayment
    {
        private readonly IRepositoryPayment paymentRepository;
        private readonly IReceiptService receiptService;
        private readonly IRentalService rentalService;
        private readonly IConversationService conversationService;

        public ServicePayment(
            IRepositoryPayment paymentRepository,
            IReceiptService receiptService,
            IRentalService rentalService,
            IConversationService conversationService)
        {
            this.paymentRepository = paymentRepository;
            this.receiptService = receiptService;
            this.rentalService = rentalService;
            this.conversationService = conversationService;
        }

        public async Task<List<PaymentDataTransferObject>> GetAllPaymentsForUI()
        {
            int currentUserId = SessionContext.GetInstance().UserId;
            var items = await BuildMergedHistoryAsync(currentUserId);
            return items.ToList();
        }

        public async Task<PagedResult<PaymentDataTransferObject>> GetFilteredPayments(
            FilterType filter,
            PaymentMethod paymentMethod = PaymentMethod.ALL,
            string searchQuery = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            int currentUserId = SessionContext.GetInstance().UserId;
            IEnumerable<PaymentDataTransferObject> items = await BuildMergedHistoryAsync(currentUserId);

            items = ApplyDtoFilters(items, paymentMethod, searchQuery, filter);
            items = ApplyDtoSorting(items, filter);

            return GetPagedResultDto(items, pageSize, pageNumber);
        }

        public decimal CalculateTotalAmount(IEnumerable<PaymentDataTransferObject> displayedPayments)
        {
            if (displayedPayments == null)
            {
                return PaymentHistoryConstants.NullAmountDefaultValue;
            }

            return displayedPayments.Sum(transaction => transaction.Amount);
        }

        public async Task<string> GetReceiptDocumentPath(int paymentId)
        {
            Payment foundPayment = await paymentRepository.GetPaymentById(paymentId);

            if (string.IsNullOrEmpty(foundPayment.ReceiptFilePath))
            {
                foundPayment.ReceiptFilePath = receiptService.GenerateReceiptRelativePath(foundPayment.RequestId);
            }
            else if (!foundPayment.ReceiptFilePath.Contains("\\"))
            {
                foundPayment.ReceiptFilePath = "receipts\\" + foundPayment.ReceiptFilePath;
            }

            return await receiptService.GetReceiptDocument(foundPayment);
        }

        public async Task<string> GetReceiptDocumentPathForRental(int rentalId)
        {
            int currentUserId = SessionContext.GetInstance().UserId;
            IEnumerable<HistoryPayment> payments = await paymentRepository.GetAllPayments();
            payments = FilterPaymentsByCurrentUser(payments);
            HistoryPayment? existing = payments.FirstOrDefault(p => p.RequestId == rentalId);

            if (existing != null)
            {
                return await GetReceiptDocumentPath(existing.TransactionIdentifier);
            }

            Rental rental = await rentalService.GetRentalById(rentalId);
            if (rental.ClientId != currentUserId && rental.OwnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("You do not have access to this rental.");
            }

            decimal paidAmount = rental.TotalPrice ?? await rentalService.GetRentalPrice(rentalId);

            var provisionalPayment = new HistoryPayment
            {
                RequestId = rentalId,
                ClientId = rental.ClientId,
                OwnerId = rental.OwnerId,
                PaidAmount = paidAmount,
                PaymentMethod = "Pending",
                DateOfTransaction = DateTime.Now,
                ReceiptFilePath = receiptService.GenerateReceiptRelativePath(rentalId),
            };

            return await receiptService.GetReceiptDocument(provisionalPayment);
        }

        private async Task<List<PaymentDataTransferObject>> BuildMergedHistoryAsync(int userId)
        {
            if (userId <= 0)
            {
                return new List<PaymentDataTransferObject>();
            }

            IEnumerable<HistoryPayment> payments = await paymentRepository.GetAllPayments();
            payments = FilterPaymentsByCurrentUser(payments);

            var rentalStatuses = await GetRentalRequestStatusMapAsync(userId);
            var paidRentalIds = payments.Select(payment => payment.RequestId).ToHashSet();

            var items = payments
                .Select(payment => MapPaymentToDto(payment, userId, rentalStatuses))
                .ToList();

            var rentals = await rentalService.GetRentalsForUser(userId);
            foreach (var rental in rentals)
            {
                if (paidRentalIds.Contains(rental.Id))
                {
                    continue;
                }

                rentalStatuses.TryGetValue(rental.Id, out var requestMessage);
                items.Add(MapRentalToDto(rental, userId, requestMessage));
            }

            return items;
        }

        private async Task<Dictionary<int, MessageDataTransferObject>> GetRentalRequestStatusMapAsync(int userId)
        {
            conversationService.Initialize(userId);
            var conversations = await conversationService.FetchConversations();

            return conversations
                .SelectMany(conversation => conversation.MessageList)
                .Where(message => message.Type == MessageType.MessageRentalRequest && message.RequestId > 0)
                .GroupBy(message => message.RequestId)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderByDescending(message => message.SentAt).First());
        }

        private static PaymentDataTransferObject MapPaymentToDto(
            HistoryPayment payment,
            int userId,
            Dictionary<int, MessageDataTransferObject> rentalStatuses)
        {
            bool isBorrowing = payment.ClientId == userId;
            string period = FormatPeriod(payment.RentalStartDate, payment.RentalEndDate);
            rentalStatuses.TryGetValue(payment.RequestId, out var requestMessage);

            return new PaymentDataTransferObject
            {
                PaymentId = payment.TransactionIdentifier,
                RentalId = payment.RequestId,
                HasPayment = true,
                SortDate = payment.DateOfTransaction ?? payment.RentalStartDate ?? DateTime.MinValue,
                DateText = payment.DateOfTransaction?.ToString("d") ?? PaymentHistoryConstants.NullDateOfTransactionDefaultValue,
                ProductName = !string.IsNullOrWhiteSpace(payment.GameName) ? payment.GameName : PaymentHistoryConstants.NullGameNameDefaultValue,
                ReceiverName = !string.IsNullOrWhiteSpace(payment.OwnerName) ? payment.OwnerName : PaymentHistoryConstants.NullOwnerNameDefaultValue,
                OtherPartyName = isBorrowing ? payment.OwnerName : payment.ClientName,
                Role = isBorrowing ? "Borrowing" : "Lending",
                Period = period,
                Status = GetPaidStatus(payment, requestMessage),
                Amount = payment.PaidAmount,
                PaymentMethod = FormatPaymentMethod(payment.PaymentMethod),
                FilePath = payment.ReceiptFilePath,
            };
        }

        private static PaymentDataTransferObject MapRentalToDto(
            RentalDataTransferObject rental,
            int userId,
            MessageDataTransferObject? requestMessage)
        {
            bool isBorrowing = rental.ClientId == userId;

            return new PaymentDataTransferObject
            {
                PaymentId = 0,
                RentalId = rental.Id,
                HasPayment = false,
                SortDate = rental.StartDate,
                DateText = rental.StartDate.ToString("d"),
                ProductName = rental.GameName,
                ReceiverName = isBorrowing ? rental.OwnerName : rental.ClientName,
                OtherPartyName = isBorrowing ? rental.OwnerName : rental.ClientName,
                Role = isBorrowing ? "Borrowing" : "Lending",
                Period = FormatPeriod(rental.StartDate, rental.EndDate),
                Status = GetRentalRequestStatus(requestMessage, userId),
                Amount = rental.Price,
                PaymentMethod = "—",
                FilePath = null,
            };
        }

        private static string FormatPeriod(DateTime? start, DateTime? end)
        {
            if (!start.HasValue || !end.HasValue)
            {
                return "—";
            }

            return $"{start.Value:dd MMM yyyy} – {end.Value:dd MMM yyyy}";
        }

        private static string FormatPaymentMethod(string? method)
        {
            if (string.IsNullOrWhiteSpace(method))
            {
                return "Unknown";
            }

            return method.Equals("CASH", StringComparison.OrdinalIgnoreCase) ? "Cash"
                : method.Contains("card", StringComparison.OrdinalIgnoreCase) ? "Card"
                : method;
        }

        private static string GetPaidStatus(HistoryPayment payment, MessageDataTransferObject? requestMessage)
        {
            if (requestMessage is { IsResolved: true, IsAccepted: true })
            {
                return "Completed";
            }

            return FormatPaymentMethod(payment.PaymentMethod) switch
            {
                "Cash" => "Paid (Cash)",
                "Card" => "Paid (Card)",
                _ => "Paid",
            };
        }

        private static string GetRentalRequestStatus(MessageDataTransferObject? message, int currentUserId)
        {
            if (message == null)
            {
                return "Pending";
            }

            if (!message.IsResolved && !message.IsAccepted)
            {
                return "Pending";
            }

            if (!message.IsResolved && message.IsAccepted)
            {
                return "Accepted";
            }

            if (message.IsResolved && !message.IsAccepted)
            {
                return message.SenderId == currentUserId ? "Cancelled" : "Declined";
            }

            return "Completed";
        }

        private IEnumerable<HistoryPayment> FilterPaymentsByCurrentUser(IEnumerable<HistoryPayment> payments)
        {
            int currentUserId = SessionContext.GetInstance().UserId;
            if (currentUserId <= 0)
            {
                return Enumerable.Empty<HistoryPayment>();
            }

            return payments.Where(payment =>
                payment.ClientId == currentUserId ||
                payment.OwnerId == currentUserId);
        }

        private static IEnumerable<PaymentDataTransferObject> ApplyDtoFilters(
            IEnumerable<PaymentDataTransferObject> items,
            PaymentMethod paymentMethod,
            string searchQuery,
            FilterType filter)
        {
            if (paymentMethod != PaymentMethod.ALL)
            {
                items = items.Where(item => MatchesPaymentMethod(item, paymentMethod));
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                items = items.Where(item =>
                    (item.ProductName ?? string.Empty).Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
            }

            items = ApplyDtoDateFilters(items, filter);
            return items;
        }

        private static bool MatchesPaymentMethod(PaymentDataTransferObject item, PaymentMethod paymentMethod)
        {
            if (!item.HasPayment)
            {
                return false;
            }

            var method = item.PaymentMethod?.ToLowerInvariant() ?? string.Empty;
            return paymentMethod switch
            {
                PaymentMethod.CASH => method.Contains("cash"),
                PaymentMethod.CARD => method.Contains("card"),
                _ => true,
            };
        }

        private static IEnumerable<PaymentDataTransferObject> ApplyDtoDateFilters(IEnumerable<PaymentDataTransferObject> items, FilterType filter)
        {
            DateTime currentDateTime = DateTime.Now;

            return filter switch
            {
                FilterType.Last3Months => items.Where(item => item.SortDate >= currentDateTime.AddMonths(-3)),
                FilterType.Last6Months => items.Where(item => item.SortDate >= currentDateTime.AddMonths(-6)),
                FilterType.Last9Months => items.Where(item => item.SortDate >= currentDateTime.AddMonths(-9)),
                _ => items,
            };
        }

        private static IEnumerable<PaymentDataTransferObject> ApplyDtoSorting(IEnumerable<PaymentDataTransferObject> items, FilterType filter)
        {
            return filter switch
            {
                FilterType.AlphabeticalAsc => items.OrderBy(item =>
                    string.IsNullOrWhiteSpace(item.ProductName) ? PaymentHistoryConstants.NullGameNameDefaultValue : item.ProductName),
                FilterType.AlphabeticalDesc => items.OrderByDescending(item =>
                    string.IsNullOrWhiteSpace(item.ProductName) ? PaymentHistoryConstants.NullGameNameDefaultValue : item.ProductName),
                FilterType.Oldest => items.OrderBy(item => item.SortDate),
                FilterType.Newest => items.OrderByDescending(item => item.SortDate),
                _ => items.OrderByDescending(item => item.SortDate),
            };
        }

        private static PagedResult<PaymentDataTransferObject> GetPagedResultDto(
            IEnumerable<PaymentDataTransferObject> items,
            int pageSize,
            int pageNumber)
        {
            var list = items.ToList();
            int totalCount = list.Count;

            return new PagedResult<PaymentDataTransferObject>
            {
                Items = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
        }
    }
}
