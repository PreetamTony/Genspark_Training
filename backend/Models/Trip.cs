using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int BusId { get; set; }
        [ForeignKey("BusId")]
        public Bus Bus { get; set; }
        
        [Required]
        public int RouteId { get; set; }
        [ForeignKey("RouteId")]
        public Route Route { get; set; }
        
        [Required]
        public DateTime DepartureTime { get; set; }
        
        [Required]
        public DateTime ArrivalTime { get; set; }
        
        [Required]
        public decimal BaseSeatPrice { get; set; }
        
        public TripStatus Status { get; set; }
    }
}
