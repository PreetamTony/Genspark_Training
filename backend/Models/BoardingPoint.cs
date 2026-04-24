using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class BoardingPoint
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ScheduleId { get; set; }
        [ForeignKey("ScheduleId")]
        public Schedule Schedule { get; set; } = null!;

        [Required]
        public int LocationId { get; set; }
        [ForeignKey("LocationId")]
        public Location Location { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string PointName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        public TimeSpan Time { get; set; }

        [MaxLength(50)]
        public string? Landmark { get; set; }

        public int Order { get; set; } = 0;
    }
}
