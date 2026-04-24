using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? State { get; set; }
    }
}
