using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Layout
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // e.g. "2x2 Seater", "1x2 Sleeper"

        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty; // "Seater" | "Sleeper"

        [Required]
        public int TotalCapacity { get; set; }

        // JSON: [ { "row": 1, "col": "A", "label": "1A", "type": "window" }, ... ]
        [Required]
        public string SeatConfigurationJson { get; set; } = string.Empty;
    }
}
