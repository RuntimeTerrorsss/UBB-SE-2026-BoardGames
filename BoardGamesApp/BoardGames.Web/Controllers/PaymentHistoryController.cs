// <copyright file="PaymentHistoryController.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BoardGames.Web.Controllers
{
    public class PaymentHistoryController : BaseController
    {
        private readonly IServicePayment servicePayment;

        public PaymentHistoryController(IServicePayment servicePaymentParam)
        {
            this.servicePayment = servicePaymentParam;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var redirect = this.RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            var userId = this.CurrentUserId ?? -1;
            BoardGames.Data.Enums.SessionContext.GetInstance().UserId = userId;

            var result = await this.servicePayment.GetFilteredPayments(
                filter: FilterType.Newest,
                paymentMethod: PaymentMethod.ALL,
                searchQuery: string.Empty,
                pageNumber: 1,
                pageSize: 10);

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
            var redirect = this.RequireLogin();
            if (redirect != null)
            {
                return this.Json(new { error = "Not logged in" });
            }

            var userId = this.CurrentUserId ?? -1;
            BoardGames.Data.Enums.SessionContext.GetInstance().UserId = userId;

            var result = await this.servicePayment.GetFilteredPayments(
                filter,
                paymentMethod,
                searchQuery,
                pageNumber,
                pageSize);

            var totalAmount = this.servicePayment.CalculateTotalAmount(result.Items);

            return this.Json(new
            {
                items = result.Items,
                totalCount = result.TotalCount,
                totalPages = result.TotalPages,
                pageNumber = result.PageNumber,
                totalAmount = totalAmount.ToString("C"),
            });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(int? paymentId, int? rentalId)
        {
            var redirect = this.RequireLogin();
            if (redirect != null)
            {
                return redirect;
            }

            BoardGames.Data.Enums.SessionContext.GetInstance().UserId = this.CurrentUserId ?? -1;

            try
            {
                string filePath;
                if (paymentId is > 0)
                {
                    filePath = await this.servicePayment.GetReceiptDocumentPath(paymentId.Value);
                }
                else if (rentalId is > 0)
                {
                    filePath = await this.servicePayment.GetReceiptDocumentPathForRental(rentalId.Value);
                }
                else
                {
                    return this.BadRequest();
                }

                if (!System.IO.File.Exists(filePath))
                {
                    return this.NotFound();
                }

                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                string fileName = Path.GetFileName(filePath);
                return this.File(fileBytes, "application/pdf", fileName);
            }
            catch (UnauthorizedAccessException)
            {
                return this.Forbid();
            }
            catch (Exception)
            {
                return this.NotFound();
            }
        }
    }
}
