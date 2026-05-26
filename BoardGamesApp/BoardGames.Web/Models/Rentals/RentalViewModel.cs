using System.ComponentModel.DataAnnotations;

namespace BoardGames.Web.Models.Rentals
{
    public class RentalViewModel
    {
        public int BookingId { get; set; }

        [Required]
        [Display(Name = "Game ID")]
        public int GameId { get; set; }

        [Required]
        [Display(Name = "Borrower ID")]
        public int BorrowerId { get; set; }

        [Required]
        [Display(Name = "Owner ID")]
        public int OwnerId { get; set; }

        [Required(ErrorMessage = "Start Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "End Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1);
    }
}
