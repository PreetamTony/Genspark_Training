using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Route
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SourceId { get; set; }
        [ForeignKey("SourceId")]
        public Location Source { get; set; } = null!;

        [Required]
        public int DestinationId { get; set; }
        [ForeignKey("DestinationId")]
        public Location Destination { get; set; } = null!;

        // Comma-separated stop names (for display)
        public string? Stops { get; set; }
    }
}
