using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BusId { get; set; }
        [ForeignKey("BusId")]
        public Bus Bus { get; set; } = null!;

        [Required]
        public int RouteId { get; set; }
        [ForeignKey("RouteId")]
        public Route Route { get; set; } = null!;

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Required]
        public decimal BasePrice { get; set; }

        public ScheduleStatus Status { get; set; } = ScheduleStatus.Scheduled;

        // Optional explicit pickup/drop if not auto-assigned from HQ
        public string? PickupPoint { get; set; }
        public string? DropPoint { get; set; }
    }
}
