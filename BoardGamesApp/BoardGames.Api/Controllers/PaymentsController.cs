// <copyright file="PaymentsController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardGames.Api.Services;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentRepository _repo;
        private readonly IRepositoryPayment _historyRepo;
        private readonly IDashboardService _dashboardService;

        public PaymentsController(IPaymentRepository repo, IRepositoryPayment historyRepo, IDashboardService dashboardService)
        {
            this._repo = repo;
            this._historyRepo = historyRepo;
            this._dashboardService = dashboardService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await this._repo.GetPaymentByIdentifierAsync(id);
            if (payment == null)
            {
                return this.NotFound();
            }

            return this.Ok(payment);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Payment>>> GetAll()
        {
            return this.Ok(await this._repo.GetAllPaymentsAsync());
        }

        [HttpGet("history")]
        public async Task<ActionResult<IReadOnlyList<HistoryPayment>>> GetHistory()
        {
            return this.Ok(await this._historyRepo.GetAllPayments());
        }

        [HttpGet("history/{id}")]
        public async Task<ActionResult<HistoryPayment>> GetHistoryById(int id)
        {
            var result = await this._historyRepo.GetPaymentById(id);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        [HttpGet("user/{accountId:guid}/history")]
        public async Task<ActionResult<List<PaymentDTO>>> GetHistoryForUser(Guid accountId)
        {
            var history = await this._dashboardService.GetPaymentHistoryForUser(accountId);
            return this.Ok(history);
        }

        [HttpPost]
        public async Task<ActionResult<int>> AddPayment([FromBody] Payment payment)
        {
            try
            {
                if (payment.DateOfTransaction == default)
                {
                    payment.DateOfTransaction = DateTime.Now;
                }

                int newId = await this._repo.AddPaymentAsync(payment);
                return this.Ok(newId);
            }
            catch (Exception ex)
            {
                return this.Problem(detail: ex.InnerException?.Message ?? ex.Message, statusCode: 500);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Payment>> UpdatePayment(int id, [FromBody] Payment payment)
        {
            payment.TransactionIdentifier = id;
            var updated = await this._repo.UpdatePaymentAsync(payment);
            if (updated == null)
            {
                return this.NotFound();
            }

            return this.Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayment(int id)
        {
            var existing = await this._repo.GetPaymentByIdentifierAsync(id);
            if (existing == null)
            {
                return this.NotFound();
            }

            bool deleted = await this._repo.DeletePaymentAsync(existing);
            return deleted ? this.NoContent() : this.StatusCode(500);
        }
    }
}
