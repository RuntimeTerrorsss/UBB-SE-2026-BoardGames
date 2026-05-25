using BoardGames.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentRepository _repo;
        private readonly IRepositoryPayment _historyRepo;

        public PaymentsController(IPaymentRepository repo, IRepositoryPayment historyRepo)
        {
            _repo = repo;
            _historyRepo = historyRepo;
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

        // Returns HistoryPayment records with GameName + OwnerName populated via JOIN.
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
