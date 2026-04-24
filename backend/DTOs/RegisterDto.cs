using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class RegisterDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public Role Role { get; set; }

        // Only required for Operator role
        public string? CompanyName { get; set; }
        
        // Head office location for Operator role
        public int? HeadOfficeLocationId { get; set; }
    }
}
