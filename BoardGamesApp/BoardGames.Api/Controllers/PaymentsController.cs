// <copyright file="PaymentsController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentRepository repo;
        private readonly IRepositoryPayment historyRepo;

        public PaymentsController(IPaymentRepository repository, IRepositoryPayment historyRepository)
        {
            this.repo = repository;
            this.historyRepo = historyRepository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await this.repo.GetPaymentByIdentifierAsync(id);
            if (payment == null)
            {
                return this.NotFound();
            }

            return this.Ok(payment);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Payment>>> GetAll()
        {
            return this.Ok(await this.repo.GetAllPaymentsAsync());
        }

        // Returns HistoryPayment records with GameName + OwnerName populated via JOIN.
        [HttpGet("history")]
        public async Task<ActionResult<IReadOnlyList<HistoryPayment>>> GetHistory()
        {
            return this.Ok(await this.historyRepo.GetAllPayments());
        }

        [HttpGet("history/{id}")]
        public async Task<ActionResult<HistoryPayment>> GetHistoryById(int id)
        {
            var result = await this.historyRepo.GetPaymentById(id);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
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

                int newId = await this.repo.AddPaymentAsync(payment);
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
            var updated = await this.repo.UpdatePaymentAsync(payment);
            if (updated == null)
            {
                return this.NotFound();
            }

            return this.Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayment(int id)
        {
            var existing = await this.repo.GetPaymentByIdentifierAsync(id);
            if (existing == null)
            {
                return this.NotFound();
            }

            bool deleted = await this.repo.DeletePaymentAsync(existing);
            return deleted ? this.NoContent() : this.StatusCode(500);
        }
    }
}
