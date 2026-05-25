// <copyright file="PaymentViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace BoardGames.Web.Models.Payment
{
    public class PaymentViewModel : IValidatableObject
    {
        public int PaymentId { get; set; }

        public int MessageId { get; set; }

        public string GameName { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public string RentalPeriod { get; set; } = string.Empty;

        public decimal AccountBalance { get; set; }

        [Required]
        public int RequestIdentifier { get; set; }

        [Required]
        public int ClientIdentifier { get; set; }

        [Required]
        public int OwnerIdentifier { get; set; }

        [Required]
        [Display(Name = "Transaction Amount")]
        [DataType(DataType.Currency)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Transaction Date")]
        [DataType(DataType.Date)]
        public DateTime DateOfTransaction { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Card Number")]
        [StringLength(23, MinimumLength = 12)]
        [RegularExpression("^[0-9\\s-]+$", ErrorMessage = "Card number must contain only digits.")]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "CVV")]
        [StringLength(4, MinimumLength = 3)]
        [RegularExpression("^\\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")]
        public string Cvv { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Cardholder Name")]
        [StringLength(100)]
        [RegularExpression("^[A-Za-z][A-Za-z\\s'\\-]*$", ErrorMessage = "Cardholder name must contain letters only.")]
        public string CardholderName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Expiry")]
        [StringLength(5, MinimumLength = 4)]
        [RegularExpression("^(0[1-9]|1[0-2])\\/\\d{2}$", ErrorMessage = "Expiry must be in MM/YY format.")]
        public string Expiry { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(this.CardNumber))
            {
                var digits = new string(this.CardNumber.Where(char.IsDigit).ToArray());
                if (digits.Length is < 12 or > 19)
                {
                    yield return new ValidationResult("Card number must be 12 to 19 digits.", new[] { nameof(this.CardNumber) });
                }
            }

            if (!string.IsNullOrWhiteSpace(this.Expiry))
            {
                if (DateTime.TryParseExact(this.Expiry, "MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var expiry))
                {
                    var lastDayOfMonth = new DateTime(expiry.Year, expiry.Month, DateTime.DaysInMonth(expiry.Year, expiry.Month));
                    if (DateTime.UtcNow.Date > lastDayOfMonth)
                    {
                        yield return new ValidationResult("Card expiry date must be in the future.", new[] { nameof(this.Expiry) });
                    }
                }
            }

            if (this.DateOfTransaction.Date > DateTime.Today)
            {
                yield return new ValidationResult("Transaction date cannot be in the future.", new[] { nameof(this.DateOfTransaction) });
            }
        }
    }
}
