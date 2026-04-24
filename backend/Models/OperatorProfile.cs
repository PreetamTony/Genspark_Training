using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class OperatorProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        public OperatorStatus Status { get; set; } = OperatorStatus.PendingApproval;

        public int? HeadOfficeLocationId { get; set; }
        [ForeignKey("HeadOfficeLocationId")]
        public Location? HeadOfficeLocation { get; set; }
    }
}
