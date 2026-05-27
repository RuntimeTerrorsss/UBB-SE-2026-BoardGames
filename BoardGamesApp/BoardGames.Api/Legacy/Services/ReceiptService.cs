// <copyright file="ReceiptService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading.Tasks;
using BookingBoardGames.Data.Constants;
using BookingBoardGames.Data.Interfaces;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Threading.Tasks;

namespace BoardGames.Api.Services
{
    public class ReceiptService : IReceiptService
    {
        private static readonly string BaseFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            ReceiptServiceConstants.BaseFolderName);

        private readonly IUserRepository userRepository;
        private readonly IRentalService rentalService;
        private readonly InterfaceGamesRepository gameRepository;

        public ReceiptService(IUserRepository userRepository, IRentalService rentalService, InterfaceGamesRepository gameRepository)
        {
            this.userRepository = userRepository;
            this.rentalService = rentalService;
            this.gameRepository = gameRepository;
        }

        /// <summary>
        /// Get a new relative path for a receipt.
        /// IMPORTANT: It does NOT create the receipt pdf.
        /// Used for assigning a unique receipt file name to transaction so pdf file can be found or created when needed.
        /// </summary>
        /// <param name="requestId">id of request for generating a unique file name</param>
        /// <returns>unique relative path allocated for the receipt</returns>
        public virtual string GenerateReceiptRelativePath(int requestId)
        {
            string fileName = $"receipt_{requestId}_{DateTime.Now:yyMMdd_HHmmss}.pdf";

            return $"receipts\\{fileName}";
        }

        /// <summary>
        /// Get the full path to the receipt pdf.
        /// Source: D:\Downloads\BookingBoardgames\receipts
        ///
        /// If pdf for receipt does not exist at source, it is created and full path to it returned.
        /// Otherwise, full path to existing pdf is returned.
        /// </summary>
        /// <param name="selectedPayment">transaction for getting relative path to receipt</param>
        /// <returns>full path to existing or newly created pdf</returns>
        /// <exception cref="InvalidOperationException">receipt path of transaction is missing</exception>
        public async Task<string> GetReceiptDocument(Payment selectedPayment)
        {
            if (selectedPayment.ReceiptFilePath == null || selectedPayment.ReceiptFilePath == string.Empty)
            {
                throw new InvalidOperationException("Receipt path is missing.");
            }

            string fullReceiptPath = GetFullPath(selectedPayment.ReceiptFilePath);

            if (!File.Exists(fullReceiptPath))
            {
                return await CreateReceipt(selectedPayment);
            }

            return fullReceiptPath;
        }

        private string PrepareDocumentPath(Payment selectedPayment)
        {
            if (string.IsNullOrWhiteSpace(selectedPayment.ReceiptFilePath))
            {
                throw new InvalidOperationException("Receipt path is missing.");
            }

            string documentPath = GetFullPath(selectedPayment.ReceiptFilePath);

            string? directoryName = Path.GetDirectoryName(documentPath);
            if (!string.IsNullOrEmpty(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            return documentPath;
        }

        private PdfDocument CreateDocument()
        {
            var createdDocument = new PdfDocument
            {
                PageLayout = PdfPageLayout.SinglePage,
            };

            createdDocument.Info.Title = ReceiptServiceConstants.DocumentTitle;

            return createdDocument;
        }

        private double DrawLine(
            XGraphics graphicsContext,
            PdfPage pdfPage,
            XFont font,
            string textLine,
            double positionX,
            double positionY)
        {
            graphicsContext.DrawString(
                textLine,
                font,
                XBrushes.Black,
                new XRect(positionX, positionY, pdfPage.Width - ReceiptServiceConstants.ContentWidthPadding, pdfPage.Height),
                XStringFormats.TopLeft);

            var textSize = graphicsContext.MeasureString(textLine, font);

            return positionY + textSize.Height;
        }

        private double DrawTextSection(
            XGraphics graphicsContext,
            PdfPage pdfPage,
            XFont font,
            string textSection,
            double currentXPosition,
            double currentYPosition)
        {
            foreach (string textLine in textSection.Split("\n"))
            {
                currentYPosition = DrawLine(
                    graphicsContext,
                    pdfPage,
                    font,
                    textLine,
                    currentXPosition,
                    currentYPosition);
            }

            return currentYPosition;
        }

        private async Task<double> DrawAllSections(
            XGraphics graphicsContext,
            PdfPage pdfPage,
            XFont font,
            Payment payment,
            double currentXPosition,
            double currentYPosition)
        {
            foreach (string textSection in await GetReceiptContent(payment))
            {
                currentYPosition = DrawTextSection(
                    graphicsContext,
                    pdfPage,
                    font,
                    textSection,
                    currentXPosition,
                    currentYPosition);

                currentYPosition += ReceiptServiceConstants.SectionSpacing;
            }

            return currentYPosition;
        }

        private async Task DrawReceiptContent(
            PdfDocument pdfDocument,
            PdfPage pdfPage,
            Payment payment)
        {
            var graphicsContext = XGraphics.FromPdfPage(pdfPage);

            var font = new XFont(
                ReceiptServiceConstants.DefaultFontFamily,
                ReceiptServiceConstants.DefaultFontSize,
                XFontStyle.Regular);

            double currentXPosition = ReceiptServiceConstants.HorizontalMargin;
            double currentYPosition = ReceiptServiceConstants.VerticalStart;

            currentYPosition = await this.DrawAllSections(
                graphicsContext,
                pdfPage,
                font,
                payment,
                currentXPosition,
                currentYPosition);
        }

        /// <summary>
        /// Creates a new pdf locally for a receipt at relative path.
        /// Destination: D:\Downloads\BookingBoardgames\receipts
        /// </summary>
        /// <param name="payment">transaction for generating the content of pdf</param>
        /// <returns>full path to created pdf</returns>
        /// <exception cref="InvalidOperationException">receipt path of transaction is missing</exception>
        private async Task<string> CreateReceipt(Payment payment)
        {
            string documentPath = PrepareDocumentPath(payment);

            using var document = CreateDocument();
            var page = document.AddPage();

            await this.DrawReceiptContent(document, page, payment);
            document.Save(documentPath);

            return documentPath;
        }

        /// <summary>
        /// Get full path from a relative path in base folder.
        /// Base folder: D:\Downloads\BookingBoardgames\
        /// </summary>
        /// <param name="relativePath">string</param>
        /// <returns>full path</returns>
        private string GetFullPath(string relativePath)
        {
            return Path.Combine(BaseFolderPath, relativePath.TrimStart('\\', '/'));
        }

        private string BuildHeader(Payment payment)
        {
            string issuedDate = GetIssuedDateFromFilename(payment.ReceiptFilePath.Split("\\")[ReceiptServiceConstants.FileNameIndexInPath]);

            return $"Receipt - Boardgame Rental\n" +
                   $"Rental ID: {payment.RequestId}\n" +
                   $"Date Issued: {issuedDate}";
        }

        private async Task<string> BuildRequestInfo(Payment payment, Rental request)
        {
            var requestedGame = await gameRepository.GetGameById(request.GameId);
            var client = await userRepository.GetById(payment.ClientId);
            var owner = await userRepository.GetById(payment.OwnerId);

            string requestInfo = $"Rental Information\n" +
                $"- Rental ID: {payment.RequestId}\n" +
                $"- Boardgame: {requestedGame.Name}\n" +
                $"- Rental Period: {request.StartDate:dd/MM/yyyy} - {request.EndDate:dd/MM/yyyy}\n" +
                $"- Client: {client.Username}\n" +
                $"- Owner: {owner.Username}";
            return requestInfo;
        }

        private string BuildPaymentDetails(Payment payment)
        {
            return $"Payment Details\n" +
                   $"- Payment Method: {payment.PaymentMethod}\n" +
                   $"- Amount Paid: {payment.PaidAmount} RON";
        }

        private string BuildConfirmation(Payment payment)
        {
            string confirmationText = "Confirmation\n";

            if (string.Equals(payment.PaymentMethod, "cash", StringComparison.OrdinalIgnoreCase))
            {
                confirmationText += $"- Owner Confirmed Payment Received: {payment.DateConfirmedSeller}\n" +
                                $"- Client Confirmed Game Received: {payment.DateConfirmedBuyer}";
            }
            else
            {
                confirmationText += $"- Payment Confirmed On: {payment.DateOfTransaction}";
            }

            return confirmationText;
        }

        private string BuildSummary()
        {
            return "Summary\n" +
                   "- the client has paid for the boardgame and the owner has acknowleded the transaction\n" +
                   "- the owner has delivered the boardgame and the client has acknowledged the delivery";
        }

        /// <summary>
        /// Get pdf content for generating the receipt pdf.
        /// </summary>
        /// <param name="payment">transaction with relevant transaction data</param>
        /// <returns>pdf content text</returns>
        private async Task<string[]> GetReceiptContent(Payment payment)
        {
            var request = await rentalService.GetRentalById(payment.RequestId);

            return new[]
            {
                BuildHeader(payment),
                await BuildRequestInfo(payment, request),
                BuildPaymentDetails(payment),
                BuildConfirmation(payment),
                BuildSummary(),
            };
        }

        /// <summary>
        /// Get formated date for "Date Issued" field in pdf content from the receipt file name.
        /// If file name has different pattern, date of today is returned.
        /// </summary>
        /// <param name="fileName">from where to extract the date</param>
        /// <returns>reformated date (dd/MM/yyyy)</returns>
        private string GetIssuedDateFromFilename(string fileName)
        {
            try
            {
                DateTime exactDate = DateTime.ParseExact(fileName.Split(ReceiptServiceConstants.FileNameSeparator)[ReceiptServiceConstants.DatePartIndex], ReceiptServiceConstants.FileDateFormat, null);
                return exactDate.ToString(ReceiptServiceConstants.DisplayDateFormat);
            }
            catch (Exception)
            {
                return DateTime.Now.ToString(ReceiptServiceConstants.DisplayDateFormat);
            }
        }
    }
}
