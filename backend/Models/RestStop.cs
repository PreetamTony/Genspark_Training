using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class RestStop
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
        public string StopName { get; set; } = string.Empty;

        [Required]
        public TimeSpan ArrivalTime { get; set; }

        [Required]
        public TimeSpan DepartureTime { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? Facilities { get; set; } // "Food, Restroom, Smoking Area"

        public int Duration => (int)(DepartureTime - ArrivalTime).TotalMinutes;
    }
}
