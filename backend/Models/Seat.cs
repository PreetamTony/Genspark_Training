using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Seat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BusId { get; set; }
        [ForeignKey("BusId")]
        public Bus Bus { get; set; } = null!;

        [Required]
        [MaxLength(10)]
        public string SeatNumber { get; set; } = string.Empty; // e.g. "1A", "2B"

        // Runtime state — set per Schedule via cache, not stored here permanently
        public SeatStatus Status { get; set; } = SeatStatus.Available;
    }
}
