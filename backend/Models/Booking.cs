using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        public int ScheduleId { get; set; }
        [ForeignKey("ScheduleId")]
        public Schedule Schedule { get; set; } = null!;

        public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;

        public decimal TotalPrice { get; set; }
        public decimal ConvenienceFee { get; set; }

        public string? CouponCode { get; set; } // Coupon code used for discount

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    }
}
