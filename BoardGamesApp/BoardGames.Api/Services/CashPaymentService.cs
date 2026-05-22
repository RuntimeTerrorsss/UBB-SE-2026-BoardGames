// <copyright file="CashPaymentService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BookingBoardGames.Data.Constants;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Sharing.Mapper;

namespace BoardGames.Api.Services
{
    public class CashPaymentService : PaymentService, ICashPaymentService
    {
        private const string CashPaymentMethod = "CASH";
        private readonly ICashPaymentMapper cashPaymentMapper;

        public CashPaymentService(
            IPaymentRepository paymentRepository,
            ICashPaymentMapper cashPaymentMapper,
            IReceiptService receiptService)
            : base(paymentRepository, receiptService)
        {
            this.cashPaymentMapper = cashPaymentMapper;
        }

        public async Task<int> AddCashPaymentAsync(CashPaymentDataTransferObject cashPaymentDataTransferObject)
        {
            Payment paymentEntity = cashPaymentMapper.TurnDataTransferObjectIntoEntity(cashPaymentDataTransferObject);
            paymentEntity.PaymentMethod = CashPaymentMethod;
            paymentEntity.PaymentState = PaymentConstrants.StateCompleted;

            int paymentIdentifier = await paymentRepository.AddPaymentAsync(paymentEntity);
            paymentEntity.TransactionIdentifier = paymentIdentifier;
            paymentEntity.ReceiptFilePath = receiptService.GenerateReceiptRelativePath(paymentEntity.RequestId);
            await paymentRepository.UpdatePaymentAsync(paymentEntity);

            return paymentIdentifier;
        }

        public async Task<CashPaymentDataTransferObject> GetCashPaymentAsync(int paymentIdentifier)
        {
            var payment = await paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);
            return cashPaymentMapper.TurnEntityIntoDataTransferObject(payment);
        }

        public async Task ConfirmDeliveryAsync(int paymentIdentifier)
        {
            Payment paymentToConfirm = await paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);
            paymentToConfirm.DateConfirmedBuyer = DateTime.Now;

            if (await IsAllConfirmedAsync(paymentIdentifier))
            {
                paymentToConfirm.ReceiptFilePath = receiptService.GenerateReceiptRelativePath(paymentToConfirm.RequestId);
            }

            await paymentRepository.UpdatePaymentAsync(paymentToConfirm);
        }

        public async Task ConfirmPaymentAsync(int paymentIdentifier)
        {
            Payment paymentToConfirm = await paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);
            paymentToConfirm.DateConfirmedSeller = DateTime.Now;

            if (await IsAllConfirmedAsync(paymentIdentifier))
            {
                paymentToConfirm.ReceiptFilePath = receiptService.GenerateReceiptRelativePath(paymentToConfirm.RequestId);
            }

            await paymentRepository.UpdatePaymentAsync(paymentToConfirm);
        }

        public async Task<bool> IsAllConfirmedAsync(int paymentIdentifier)
        {
            Payment paymentEntity = await paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);

            if (paymentEntity.DateConfirmedSeller != null && paymentEntity.DateConfirmedBuyer != null)
            {
                paymentEntity.PaymentState = PaymentConstrants.StateConfirmed;

                return true;
            }

            return false;
        }

        public async Task<bool> IsDeliveryConfirmedAsync(int paymentIdentifier)
        {
            Payment paymentEntity = await paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);

            if (paymentEntity.DateConfirmedBuyer != null)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> IsPaymentConfirmedAsync(int paymentIdentifier)
        {
            Payment paymentEntity = await paymentRepository.GetPaymentByIdentifierAsync(paymentIdentifier);

            if (paymentEntity.DateConfirmedSeller != null)
            {
                return true;
            }

            return false;
        }
    }
}
