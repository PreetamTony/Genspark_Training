using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class BookingSeat
    {
        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; } = null!;

        public int SeatId { get; set; }
        [ForeignKey("SeatId")]
        public Seat Seat { get; set; } = null!;
    }
}
