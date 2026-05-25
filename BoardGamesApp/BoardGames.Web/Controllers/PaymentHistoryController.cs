using BoardGames.Data.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    public class PaymentHistoryController : BaseController
    {
        private readonly IServicePayment _servicePayment;

        public PaymentHistoryController(IServicePayment servicePayment)
        {
            _servicePayment = servicePayment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            var userId = CurrentUserId ?? -1;
            BoardGames.Data.Enums.SessionContext.GetInstance().UserId = userId;

            var result = await _servicePayment.GetFilteredPayments(
                filter: FilterType.Newest,
                paymentMethod: PaymentMethod.ALL,
                searchQuery: string.Empty,
                pageNumber: 1,
                pageSize: 10
            );

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Filter(
            FilterType filter,
            PaymentMethod paymentMethod,
            string searchQuery = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            var redirect = RequireLogin();
            if (redirect != null) return Json(new { error = "Not logged in" });

            var userId = CurrentUserId ?? -1;
            BoardGames.Data.Enums.SessionContext.GetInstance().UserId = userId;


            var result = await _servicePayment.GetFilteredPayments(
                filter,
                paymentMethod,
                searchQuery,
                pageNumber,
                pageSize
            );

            var totalAmount = _servicePayment.CalculateTotalAmount(result.Items);

            return Json(new
            {
                items = result.Items,
                totalCount = result.TotalCount,
                totalPages = result.TotalPages,
                pageNumber = result.PageNumber,
                totalAmount = totalAmount.ToString("C")
            });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(int? paymentId, int? rentalId)
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            BoardGames.Data.Enums.SessionContext.GetInstance().UserId = CurrentUserId ?? -1;

            try
            {
                string filePath;
                if (paymentId is > 0)
                {
                    filePath = await _servicePayment.GetReceiptDocumentPath(paymentId.Value);
                }
                else if (rentalId is > 0)
                {
                    filePath = await _servicePayment.GetReceiptDocumentPathForRental(rentalId.Value);
                }
                else
                {
                    return BadRequest();
                }

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                string fileName = Path.GetFileName(filePath);
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}