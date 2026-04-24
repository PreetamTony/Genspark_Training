using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class PlatformConfig
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Key { get; set; } = string.Empty;
        [Required]
        public string Value { get; set; } = string.Empty;
    }
}
