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
            _repo = repo;
            _historyRepo = historyRepo;
            _dashboardService = dashboardService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _repo.GetPaymentByIdentifierAsync(id);
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Payment>>> GetAll()
        {
            return Ok(await _repo.GetAllPaymentsAsync());
        }

        [HttpGet("history")]
        public async Task<ActionResult<IReadOnlyList<HistoryPayment>>> GetHistory()
        {
            return Ok(await _historyRepo.GetAllPayments());
        }

        [HttpGet("history/{id}")]
        public async Task<ActionResult<HistoryPayment>> GetHistoryById(int id)
        {
            var result = await _historyRepo.GetPaymentById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("user/{accountId:guid}/history")]
        public async Task<ActionResult<List<PaymentDTO>>> GetHistoryForUser(Guid accountId)
        {
            var history = await _dashboardService.GetPaymentHistoryForUser(accountId);
            return Ok(history);
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

                int newId = await _repo.AddPaymentAsync(payment);
                return Ok(newId);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.InnerException?.Message ?? ex.Message, statusCode: 500);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Payment>> UpdatePayment(int id, [FromBody] Payment payment)
        {
            payment.TransactionIdentifier = id;
            var updated = await _repo.UpdatePaymentAsync(payment);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayment(int id)
        {
            var existing = await _repo.GetPaymentByIdentifierAsync(id);
            if (existing == null) return NotFound();
            bool deleted = await _repo.DeletePaymentAsync(existing);
            return deleted ? NoContent() : StatusCode(500);
        }
    }
}
