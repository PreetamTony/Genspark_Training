using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class BusReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BusId { get; set; }
        [ForeignKey("BusId")]
        public Bus Bus { get; set; } = null!;

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Rating categories
        public int StaffBehavior { get; set; } = 0;
        public int Punctuality { get; set; } = 0;
        public int Cleanliness { get; set; } = 0;
        public int SeatComfort { get; set; } = 0;
        public int Driving { get; set; } = 0;
        public int AC { get; set; } = 0;
        public int LiveTracking { get; set; } = 0;
        public int RestStopHygiene { get; set; } = 0;
    }
}
